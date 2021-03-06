﻿using System;
using System.Collections.Generic;
using TripLine.Dtos;

namespace TripLine.Service
{

    public class PhotoRepoContent
    {
        internal int NewId { get; set; } = 1;

        public int NewSessionId { get; set; } = 1;

        public DateTime LastFileDetectionTime { get; set; } = DateTime.MinValue;

        public List<string> ExcludedFileKeys { get; set; } = new List<string>();

        public List<Photo> Photos { get; set; } = new List<Photo>();

    }


    public class PhotoRepo : FileRepo<PhotoRepoContent>
    {
        public PhotoRepo() : this(forceNew:false)
        {
        }

        public PhotoRepo(bool forceNew=false) : base(TripLineConfig.PhotoRepoPath, forceNew)
        {
            Load();
        }
        

        public int GetNewId()
        {
            return Content.NewId++;
        }

    }
}