using System;
using System.Collections.Generic;
using System.Text;
using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Domain.Position_context
{
    public class Position
    {
        public Guid Id { get; }
        public PositionName Name { get; private set; }
        public PositionDescription Description { get; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }
        private Position(Guid id, PositionName name, PositionDescription description)
        {
            Id = id;
            Name = name;
            Description = description;
            IsActive = true;
            DateTime now = DateTime.UtcNow;
            
            CreatedAt = now;
            UpdatedAt = now;
        }
        internal static Position Create(Guid id, string name, string description)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Индентификатор должности не должен быть пустым.", 
                    nameof(id));
            }
            PositionName validName = PositionName.Create(name);
            PositionDescription validDescription =
        PositionDescription.Create(description);

             return new Position(id,validName,validDescription);
        }

        internal void Rename(PositionName name)
        {
            EnsureActive();
            Name = name;
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
                throw new InvalidOperationException("Архивированную должность нельзя изменять.");
        }
    }
}
