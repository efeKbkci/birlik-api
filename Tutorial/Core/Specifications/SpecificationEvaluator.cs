using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.Core.Specifications;

// Spesifikasyonları SQL sorgularına çevirecek olan sınıf. 
public class SpecificationEvaluator<TEntity> where TEntity : class
{
    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> spec)
    {
        var query = inputQuery;

        // Listemizin içindeki her bir lego parçasını (kuralı) sırayla ekliyoruz
        foreach (var criteria in spec.Criterias)
        {
            query = query.Where(criteria);
        }

        return query;
    }
}
