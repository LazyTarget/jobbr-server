﻿using System;
using AutoMapper;
using Jobbr.ComponentModel.Execution;
using Jobbr.ComponentModel.Execution.Model;
using Jobbr.Server.Storage;

namespace Jobbr.Server.ComponentServices.Execution
{
    internal class JobRunInformationService : IJobRunInformationService
    {
        private readonly IJobbrRepository jobbrRepository;
        private readonly IMapper mapper;

        public JobRunInformationService(IJobbrRepository jobbrRepository, IMapper mapper)
        {
            this.jobbrRepository = jobbrRepository;
            this.mapper = mapper;
        }

        public JobRunInfo GetByUniqueId(Guid uniqueId)
        {
            var jobRun = this.jobbrRepository.GetJobRunById(uniqueId);

            if (jobRun == null)
            {
                return null;
            }

            throw new NotImplementedException();
        }

        public JobRunInfo GetByJobRunId(long jobRunId)
        {
            var jobRun = this.jobbrRepository.GetJobRunById(jobRunId);

            if (jobRun == null)
            {
                return null;
            }

            var trigger = this.jobbrRepository.GetTriggerById(jobRun.TriggerId);
            var job = this.jobbrRepository.GetJob(jobRun.Id);

            var info = new JobRunInfo();

            this.mapper.Map(job, info);
            this.mapper.Map(trigger, info);
            this.mapper.Map(jobRun, info);

            return info;
        }
    }
}