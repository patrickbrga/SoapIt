using System.ServiceModel;

namespace SoapIt;

[ServiceContract(Namespace = "http://servicos.embratec.com.br/esb")]
public interface ISoapClient;