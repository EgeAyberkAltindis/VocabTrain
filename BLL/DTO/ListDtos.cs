using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTO
{
    public class ListDtos
    {
        /// <summary>
        /// Verilen adla bir liste varsa onu döner, yoksa oluşturup döner.
        /// UI’nin "liste zaten var mı, yeni mi oluşturuldu?" bilgisini göstermesi için.
        /// </summary>
        public class EnsureListResultDto
        {
            public int WordListId { get; set; }
            public string Name { get; set; } = null!;
            public bool IsCreated { get; set; } // true: yeni oluşturuldu, false: zaten vardı
        }
    }
}
