using System;
using System.Collections.Generic;
using System.Text;
using DirectoryService.Domain.ValueObjects;
namespace DirectoryService.Domain.Department_context
{
   
        
    public class Department
    {
        public Guid Id { get; }
        public DepartmentName Name { get; }
        public DepartmentIdentifier Identifier { get; }
        public Guid? ParentId { get; }
        public DepartmentPath Path { get; }
        public short Depth => Path.Depth;
        public bool IsActive { get; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; }
        private Department(Guid id, DepartmentName name, DepartmentIdentifier identifier, Department? parent)
        {
            Id = id;
            Name = name;
            Identifier = identifier;
            ParentId = parent?.Id;
            Path = parent is null ? DepartmentPath.CreateRoot(id) : parent.Path.Append(id);
            IsActive = true;

            DateTime now = DateTime.UtcNow;
            CreatedAt = now;
            UpdatedAt = now;
        }
        public static Department Create(Guid id, string name, string identifier, Department? parent = null)
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
    }
}
