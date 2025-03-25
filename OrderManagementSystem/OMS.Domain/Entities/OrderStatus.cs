using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Domain.Entities
{
    [Table("OrderStatus")]
    public class OrderStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
