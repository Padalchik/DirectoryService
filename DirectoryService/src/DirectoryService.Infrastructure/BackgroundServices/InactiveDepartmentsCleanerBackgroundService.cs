using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.BackgroundServices;

public class InactiveDepartmentsCleanerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InactiveDepartmentsCleanerBackgroundService> _logger;

    public InactiveDepartmentsCleanerBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<InactiveDepartmentsCleanerBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("InactiveDepartmentsCleanerBackgroundService started");

        // Таймер с периодом
        using var timer = new PeriodicTimer(TimeSpan.FromDays(1));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation(
                        "Background service tick at {Time}",
                        DateTime.UtcNow);

                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

                    string sql = GetSqlString();
                    await dbContext.Database.ExecuteSqlRawAsync(sql, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cleanup execution failed");
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            // Нормально при остановке приложения
        }

        _logger.LogInformation("InactiveDepartmentsCleanupService stopped");
    }

    private static string GetSqlString()
    {
        string sql = $"""
                      WITH to_delete AS (
                          SELECT id, path
                          FROM public.departments
                          WHERE is_active = false
                            AND deleted_at IS NOT NULL
                            AND deleted_at < now() - interval '1 minute'
                      ),
                      
                      -- все потомки удаляемых (любой глубины)
                      descendants AS (
                          SELECT d.*
                          FROM public.departments d
                          JOIN to_delete td ON d.path <@ td.path AND d.id <> td.id
                      ),
                      
                      -- считаем новый path
                      resolved AS (
                          SELECT 
                              d.id,
                      
                              (
                                  array_to_string(
                                      ARRAY(
                                          SELECT seg
                                          FROM unnest(string_to_array(d.path::text, '.')) AS seg
                                          WHERE seg NOT LIKE 'deleted-%'
                                      ),
                                      '.'
                                  )
                              )::ltree AS new_path
                          FROM descendants d
                      ),
                      
                      -- считаем parent_path
                      fin AS (
                          SELECT 
                              r.id,
                              r.new_path,
                      
                              CASE
                                  WHEN nlevel(r.new_path) > 1 
                                       THEN subpath(r.new_path, 0, nlevel(r.new_path)-1)
                                  ELSE NULL
                              END AS parent_path
                          FROM resolved r
                      ),
                      
                      -- тут ВАЖНО: ищем родителя среди уже пересчитанных путей
                      upd AS (
                          UPDATE public.departments d
                          SET
                              path  = f.new_path,
                              depth = nlevel(f.new_path) - 1,
                              parent_id = p.id
                          FROM fin f
                          LEFT JOIN fin p 
                              ON p.new_path = f.parent_path
                          WHERE d.id = f.id
                      ),
                      
                      -- Удаляем связанные locations
                      del_locations AS (
                        DELETE FROM public.department_locations dl
                        USING to_delete td
                        WHERE dl.department_id = td.id
                      ),
                                            
                      -- Удаляем связанные positions
                      del_positions AS (
                        DELETE FROM public.department_positions dp
                        USING to_delete td
                        WHERE dp.department_id = td.id
                      )
                      
                      -- Удаляем departments
                      DELETE FROM public.departments d
                      USING to_delete t
                      WHERE d.id = t.id;
                      """;

        return sql;
    }
}
