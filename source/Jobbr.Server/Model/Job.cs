﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Jobbr.Server.Model
{
    public class Job
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string ParametersRaw { get; set; }

        public DateTime CreatedDateTimeUtc { get; set; }

        public DateTime? UpdatedDateTimeUtc { get; set; }
    }
}