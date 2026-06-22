using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public abstract class MAPPER<T>
    {        
        public abstract void Insertar(T obj);

        public abstract void Updatear(T obj);

        public abstract void Deletear(T obj);

        public abstract List<T> Listar();

    }
}
