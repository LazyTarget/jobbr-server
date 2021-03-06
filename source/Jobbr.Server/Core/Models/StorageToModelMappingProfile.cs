﻿using AutoMapper;
using Jobbr.ComponentModel.ArtefactStorage.Model;

namespace Jobbr.Server.Core.Models
{
    internal class StorageToModelMappingProfile : Profile
    {
        public StorageToModelMappingProfile()
        {
            this.CreateMap<JobbrArtefact, JobArtefactModel>();
        }
    }
}
