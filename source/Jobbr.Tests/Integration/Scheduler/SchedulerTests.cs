﻿using System;
using System.Linq;
using Jobbr.ComponentModel.Management.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Job = Jobbr.ComponentModel.JobStorage.Model.Job;
using JobRunStates = Jobbr.ComponentModel.JobStorage.Model.JobRunStates;

namespace Jobbr.Tests.Integration.Scheduler
{
    [TestClass]
    public class SchedulerTests : RunningJobbrServerTestBase
    {
        [TestMethod]
        public void JobRunIsScheduled_RecurringTriggerGetsUpdated_JobRunScheduleIsAdjusted()
        {
            var jobManagementService = this.Services.JobManagementService;
            var storageProvider = this.Services.JobStorageProvider;

            var demoJob = new Job();
            storageProvider.AddJob(demoJob);

            var currentMinutesInHour = DateTime.UtcNow.Minute;
            var futureMinute = (currentMinutesInHour + 5) % 60;
            var futureMinute2 = (futureMinute + 2) % 60;

            var recurringTrigger = new RecurringTrigger { JobId = 1, Definition = futureMinute + " * * * *", IsActive = true };
            jobManagementService.AddTrigger(recurringTrigger);

            // Wait for the scheduler to do his work
            WaitFor.HasElements(() => storageProvider.GetJobRuns());

            var createdJobRun = storageProvider.GetJobRuns().FirstOrDefault();

            // Base Assertions
            Assert.IsNotNull(createdJobRun, "There should be exact one JobRun which is not null");
            Assert.IsTrue(createdJobRun.PlannedStartDateTimeUtc >= DateTime.UtcNow, "The job run needs to be in the future");
            Assert.AreEqual(futureMinute, createdJobRun.PlannedStartDateTimeUtc.Minute);

            jobManagementService.UpdateTriggerDefinition(recurringTrigger.Id, futureMinute2 + " * * * *");

            // Wait for the scheduler to do his work
            WaitFor.HasElements(() => storageProvider.GetJobRuns().Where(r => r.PlannedStartDateTimeUtc.Minute == futureMinute2).ToList());

            var updatedJobRun = storageProvider.GetJobRuns().FirstOrDefault();

            Assert.AreEqual(futureMinute2, updatedJobRun.PlannedStartDateTimeUtc.Minute, "As per updated definition, the job should now start on a different plan");
        }

        [TestMethod]
        public void JobRunIsScheduler_JobRunWillBeRemoved_WhenTriggerGetsDisabled()
        {
            var jobManagementService = this.Services.JobManagementService;
            var storageProvider = this.Services.JobStorageProvider;

            var demoJob = new Job();
            storageProvider.AddJob(demoJob);

            var futureDate1 = DateTime.UtcNow.AddHours(2);

            var trigger = new ScheduledTrigger { JobId = demoJob.Id, StartDateTimeUtc = futureDate1, IsActive = true };
            jobManagementService.AddTrigger(trigger);

            // Wait for the scheduler to do his work
            WaitFor.HasElements(() => storageProvider.GetJobRuns());

            var createdJobRun = storageProvider.GetJobRuns().FirstOrDefault();
            Assert.IsNotNull(createdJobRun, "There should be exact one JobRun");
            Assert.IsTrue(createdJobRun.PlannedStartDateTimeUtc >= DateTime.UtcNow, "The job run needs to be in the future");
            Assert.AreEqual(futureDate1, createdJobRun.PlannedStartDateTimeUtc);

            jobManagementService.DisableTrigger(trigger.Id);

            // Wait for the scheduler to do his work
            WaitFor.HasElements(() => storageProvider.GetJobRuns().Where(r => r.State == ComponentModel.JobStorage.Model.JobRunStates.Deleted).ToList());

            var jobRun = storageProvider.GetJobRuns().FirstOrDefault();

            Assert.AreEqual(ComponentModel.JobStorage.Model.JobRunStates.Deleted, jobRun.State);
        }

        [TestMethod]
        public void JobRunIsScheduler_JobRunWillBeScheduled_WhenTriggerIsEnabled()
        {
            var jobManagementService = this.Services.JobManagementService;
            var storageProvider = this.Services.JobStorageProvider;

            var demoJob = new Job();
            storageProvider.AddJob(demoJob);

            var futureDate1 = DateTime.UtcNow.AddHours(2);

            var trigger = new ScheduledTrigger { JobId = demoJob.Id, StartDateTimeUtc = futureDate1, IsActive = false };
            jobManagementService.AddTrigger(trigger);

            // Base asserts
            var createdJobRun = storageProvider.GetJobRuns().FirstOrDefault();
            Assert.IsNull(createdJobRun, "There should be exact no JobRun");

            jobManagementService.EnableTrigger(trigger.Id);

            // Wait for the scheduler to do his work
            WaitFor.HasElements(() => storageProvider.GetJobRuns());

            var jobRun = storageProvider.GetJobRuns().FirstOrDefault();

            Assert.AreEqual(JobRunStates.Scheduled, jobRun.State);
        }
    }
}