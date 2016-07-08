using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace CalculatorService
{
    [ServiceContract]
    public interface ICalculatorService
    {
        [OperationContract]
        [WebGet]
        Task<int> Add(int a, int b);
    }
}