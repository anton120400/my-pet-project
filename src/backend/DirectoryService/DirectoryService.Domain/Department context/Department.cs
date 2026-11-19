using System;
using System.Collections.Generic;
using System.Text;
using DirectoryService.Domain.ValueObjects;
namespace DirectoryService.Domain.Department_context
{
   
        
    public class Department
    {
        public Guid Id { get; }
        public DepartmentName Name { get; private set; }
        public DepartmentIdentifier Identifier { get; private set; }
        public Guid? ParentId { get; private set; }
        public DepartmentPath Path { get; private set; }
        public short Depth => Path.Depth;
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; private set; }
        private Department(Guid id, DepartmentName name, DepartmentIdentifier identifier, Department? parent)
        {
            Id = id;
            Name = name;
            Identifier = identifier;
            ParentId = parent?.Id;
            Path = parent is null
                ? DepartmentPath.CreateRoot(identifier)
                : parent.Path.Append(identifier);
            IsActive = true;

            DateTime now = DateTime.UtcNow;
            CreatedAt = now;
            UpdatedAt = now;
        }
        internal static Department Create(Guid id, string name, string identifier, Department? parent)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                "Идентификатор подразделения не должен быть пустым.",
                nameof(id));
            }
            DepartmentName validName = DepartmentName.Create(name);
            DepartmentIdentifier validIdentifier =
                DepartmentIdentifier.Create(identifier);

            return new Department(id, validName, validIdentifier, parent);
        }

        internal void Update(DepartmentName name, DepartmentIdentifier identifier)
        {
            EnsureActive();
            Name = name;
            Identifier = identifier;
            MarkChanged();
        }

        internal void SetHierarchy(Guid? parentId, DepartmentPath path)
        {
            EnsureActive();
            ParentId = parentId;
            Path = path;
            MarkChanged();
        }

        public void Archive()
        {
            EnsureActive();
            IsActive = false;
            MarkChanged();
        }

        internal void EnsureActive()
        {
            if (!IsActive)
                throw new InvalidOperationException("Архивированное подразделение нельзя изменять.");
        }

        internal void MarkChanged() => UpdatedAt = DateTime.UtcNow;
    }
}
