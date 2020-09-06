using Ingvilt.Models;
using Ingvilt.Util;
using Newtonsoft.Json;
using System;

namespace Ingvilt.Dto.Tags {
    public class Tag : IComparable<Tag>, INamedEntity {
        [JsonIgnore]
        public long TagId {
            get;
            set;
        }

        public string Name {
            get;
            set;
        }

        public string Type {
            get;
            set;
        }

        public string UniqueId {
            get;
            set;
        }

        public Tag(long tagId, string name, string type, string uniqueId = null) {
            TagId = tagId;
            Name = name;
            Type = type;

            if (uniqueId == null) {
                uniqueId = UniqueIdUtil.GenerateUniqueId();
            }
            UniqueId = uniqueId;
        }

        public Tag(long tagId, CreateTagDto dto) : this(tagId, dto.Name, dto.Type) {
        }

        private Tag() {
        }

        public override bool Equals(object other) {
            var otherTag = other as Tag;
            if (otherTag == null) {
                return false;
            }

            return otherTag.TagId == TagId && otherTag.Name == Name && otherTag.Type == Type;
        }

        public override int GetHashCode() {
            return ((int)TagId << 2) ^ Name.GetHashCode();
        }

        public int CompareTo(Tag other) {
            return Name.CompareTo(other.Name);
        }

        public string GetName() {
            return Name;
        }
    }
}
