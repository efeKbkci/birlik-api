using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.Core.Specifications;

// Aşağıdaki sınıfın soyut (abstract) olarak tanımlanmasının sebebi, bu sınıftan nesne oluşturulmasını engellemektir.
public abstract class BaseSpecification<T> : ISpecification<T>
{
    public List<Expression<Func<T, bool>>> Criterias { get; } = [];

    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criterias.Add(criteria);
    }

    // Eğer filtresiz (her şeyi getiren) bir spesifikasyon lazımsa boş constructor kullanırız
    protected BaseSpecification()
    {
    }

    // Metot üzerinden yeni kriter ekleme. 
    protected void AddCriteria(Expression<Func<T, bool>> criteria)
    {
        Criterias.Add(criteria);
    }
}
