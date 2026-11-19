using System;
using System.Collections.Generic;
using System.Text;
using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain.Location_context
{
    public class Location
    { 
        public Guid Id { get; }
        public LocationName Name { get; private set; }
        public LocationAddress Address { get; private set; }
        public LocationTimeZone TimeZone { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }
        
        private Location(Guid id, LocationName name, LocationAddress address, LocationTimeZone timeZone)
        {
            Id = id;
            Name = name;
            Address = address;
            TimeZone = timeZone;
            DateTime now = DateTime.UtcNow;
            CreatedAt = now;
            UpdatedAt = now;

        }
        public static Location Create(Guid id, string name, string address, string timeZone)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Индентификатор локации не должен быть пустым.", 
                    nameof(id));
            }
            LocationName validName = LocationName.Create(name);
            LocationAddress validAddress = LocationAddress.Create(address);
            LocationTimeZone validTimeZone = LocationTimeZone.Create(timeZone);

            return new Location(id, validName, validAddress,validTimeZone);

        }

        internal void Update(LocationName name, LocationAddress address, LocationTimeZone timeZone)
        {
            EnsureActive();
            Name = name;
            Address = address;
            TimeZone = timeZone;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Archive()
        {
            EnsureActive();
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        internal void EnsureActive()
        {
            if (!IsActive)
                throw new InvalidOperationException("Архивированную локацию нельзя изменять.");
        }
    }
}

