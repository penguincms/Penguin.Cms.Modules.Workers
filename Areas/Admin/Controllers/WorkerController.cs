using Microsoft.AspNetCore.Mvc;
using Penguin.Cms.Logging;
using Penguin.Cms.Logging.Extensions;
using Penguin.Cms.Modules.Admin.Areas.Admin.Controllers;
using Penguin.Cms.Web.Extensions;
using Penguin.Files.Services;
using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Reflection;
using Penguin.Security.Abstractions.Constants;
using Penguin.Security.Abstractions.Interfaces;
using Penguin.Web.Security.Attributes;
using Penguin.Workers.Abstractions;
using Penguin.Workers.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Penguin.Cms.Modules.Workers.Areas.Admin.Controllers
{
    [RequiresRole(RoleNames.SYS_ADMIN)]
    public class WorkerController : AdminController
    {
        protected FileService FileService { get; set; }
        protected IRepository<LogEntry> LogEntryRepository { get; set; }

        protected WorkerRepository WorkerRepository { get; set; }

        public WorkerController(WorkerRepository workerRepository, IRepository<LogEntry> logEntryRepository, IServiceProvider serviceProvider, FileService fileService, IUserSession userSession) : base(serviceProvider, userSession)
        {
            FileService = fileService;
            LogEntryRepository = logEntryRepository;
            WorkerRepository = workerRepository;
        }

        public ActionResult ListWorkers()
        {
            List<IWorker?> ExistingWorkers = TypeFactory.GetAllImplementations<IWorker>().Select(t => ServiceProvider.GetService(t) as IWorker).ToList();

            foreach (IWorker? thisWorker in ExistingWorkers)
            {
                thisWorker?.UpdateLastRun();
            }

            return View(ExistingWorkers);
        }

        public ActionResult Log(string WorkerType)
        {
            List<LogEntry> Entries = LogEntryRepository.GetByCaller(WorkerType);
            return View("logs", Entries);
        }

        public ActionResult Logs()
        {
            List<LogEntry> Entries = LogEntryRepository.All.ToList();
            return View(Entries);
        }

        public ActionResult LogSessions(string Session)
        {
            List<LogEntry> Entries = LogEntryRepository.GetBySession(Session);
            return View(Entries);
        }

        public ActionResult Run(string WorkerName)
        {
            foreach (Type t in TypeFactory.GetAllImplementations(typeof(IWorker)))
            {
                if (t.FullName == WorkerName)
                {
                    IWorker worker = (IWorker)ServiceProvider.GetService(t);
                    worker.WorkerRoot = Path.Combine(FileService.ApplicationPath, "WorkerProcess");
                    worker.UpdateSync(true);
                }
            }

            this.AddMessage("Worker has completed");

            return Redirect(HttpContext.Request.Headers["Referer"].ToString() ?? "/Admin/Worker/ListWorkers");
        }
    }
}