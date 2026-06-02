using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial.Core.Specifications;

public interface ISpecification<T>
{
    /* Func<T, bool> : T türünde bir nesne alan ve bool döndüren bir delege türüdür. 
     * Örneğin Func<Trip, bool> kural = t => t.CompanyId == 5;
     * C# bu fonksiyonu derler ve belleğe alır. 
     * Ancak bu fonksiyonun SQL'e çevrilmesi mümkün değildir. Çünkü SQL, C#'ın derlenmiş kodunu anlayamaz.
     * Expression ifadesi sayesinde C# bu kodu doğrudan derleyip bir kara kutu ((çalıştırılabilir kod)) haline getirmez. 
     * Bunun yerine, kodu bir veri yapısı olarak temsil eder. 
     * Veri yapısı bir reçete gibidir. EF Core bu reçeteyi okuyarak uygun SQL sorgusunu oluşturur. 
     */
    List<Expression<Func<T, bool>>> Criterias { get; }
}
