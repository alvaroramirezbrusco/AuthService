using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Request
{
    public class ChangeUserRoleRequest
    {
        public Guid UserId { get; set; }
        public int NewRole { get; set; }
    }
}
