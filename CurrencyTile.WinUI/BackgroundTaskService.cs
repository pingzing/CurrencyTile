using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CurrencyTile.Shared;
using Serilog;
using Windows.ApplicationModel.Background;

namespace CurrencyTile.WinUI;

public class BackgroundTaskService
{
    private Lazy<Task<ILogger>> _logger = new(() => new Logging().GetLogger(), false);
    private const string TimerUpdateTaskName = "UpdateTilesTimerTask";

    public async Task<IBackgroundTaskRegistration?> GetOrRegisterTimerTask()
    {
        var status = await BackgroundExecutionManager.RequestAccessAsync();
        if (
            status == BackgroundAccessStatus.Unspecified
            || status == BackgroundAccessStatus.DeniedByUser
            || status == BackgroundAccessStatus.DeniedBySystemPolicy
        )
        {
            (await _logger.Value).Error(
                "Unable to register background tasks. Access is denied. Specifically: {status}",
                status
            );
            return null;
        }

        foreach (var task in BackgroundTaskRegistration.AllTasks)
        {
            if (task.Value.Name == TimerUpdateTaskName)
            {
                return task.Value;
            }
        }

        // No tasks with that name found. Register a new one.
        var builder = new BackgroundTaskBuilder
        {
            Name = TimerUpdateTaskName,
            TaskEntryPoint = "CurrencyTile.TimerTask.UpdateTilesTask",
            IsNetworkRequested = true
        };
        builder.SetTrigger(new TimeTrigger(60, oneShot: false));

        var registration = builder.Register();
        return registration;
    }

    private const string AppTrigerTaskName = "UpdateTilesAppTriggerTask";

    public async Task<IBackgroundTaskRegistration?> GetOrRegisterAppTriggerTask()
    {
        var status = await BackgroundExecutionManager.RequestAccessAsync();
        if (
            status == BackgroundAccessStatus.Unspecified
            || status == BackgroundAccessStatus.DeniedByUser
            || status == BackgroundAccessStatus.DeniedBySystemPolicy
        )
        {
            (await _logger.Value).Error(
                "Unable to register background tasks. Access is denied. Specifically: {status}",
                status
            );
            return null;
        }

        foreach (var task in BackgroundTaskRegistration.AllTasks)
        {
            if (task.Value.Name == AppTrigerTaskName)
            {
                return task.Value;
            }
        }

        // No tasks with that name found. Register a new one.
        var builder = new BackgroundTaskBuilder
        {
            Name = AppTrigerTaskName,
            TaskEntryPoint = "CurrencyTile.TimerTask.UpdateTilesTask",
            IsNetworkRequested = true
        };
        builder.SetTrigger(new ApplicationTrigger());

        var registration = builder.Register();
        return registration;
    }

    public void UnregisterAll()
    {
        foreach (var item in BackgroundTaskRegistration.AllTasks)
        {
            item.Value.Unregister(false);
        }
    }
}
