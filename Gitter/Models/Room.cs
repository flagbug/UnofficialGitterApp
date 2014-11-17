﻿using System;

namespace Gitter.Models
{
    public class Room : IEquatable<Room>
    {
        public string githubType { get; set; }

        public string id { get; set; }

        public string lastAccessTime { get; set; }

        public bool lurk { get; set; }

        public int mentions { get; set; }

        public string name { get; set; }

        public bool oneToOne { get; set; }

        public string security { get; set; }

        public string topic { get; set; }

        public int unreadItems { get; set; }

        public string uri { get; set; }

        public string url { get; set; }

        public User user { get; set; }

        public int? userCount { get; set; }

        public int? v { get; set; }

        public bool Equals(Room other)
        {
            return other != null && this.id == other.id;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Room);
        }

        public override int GetHashCode()
        {
            return new { this.id }.GetHashCode();
        }
    }
}