using ChapterFST2.Controllers;
using ChapterFST2.Interfaces;
using ChapterFST2.Models;
using ChapterFST2.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace TesteIntegracao.Controllers
{
    public class LoginControllerTeste
    {
        [Fact]
        public void LoginController_Retornar_UsuarioInvalido()
        {
            //Arrange (prepara��o). Primeiro, precisamos espelhar o reposit�rio (n�o iremos usar o original, pois n�o iremos fazer a conex�o direta com o banco de dados) e configurar o m�todo login para o que desejamos receber.
            var repositorioEspelhado = new Mock<IUsuarioRepository>();

            //atrav�s do m�todo 'Setup' acessamos o m�todo login do reposit�rio original. Como iremos configurar o retorno do m�todo login, n�o importa o argumento email e senha, por isso usamos "It.IsAny<string>()" (isso � qualquer string). Duas vezes porque o m�todo login solicita duas strings (email e senha).
            repositorioEspelhado.Setup(x => x.Login(It.IsAny<string>(), It.IsAny<string>())).Returns((Usuario)null); //O �ltimo m�todo � para obter como retorno um usu�rio nulo.

            //Quando chamamos o m�todo login, � esperado receber um objeto do tipo LoginViewModel com os dados que ir� buscar no banco. Ent�o, precisamos criar um objeto desse tipo.
            LoginViewModel dados = new LoginViewModel();
            dados.Email = "email_inexistente@email.com";
            dados.Senha = "09876";

            //Agora, � preciso criar uma inst�ncia do controller para chamar o loginController
            var controller = new LoginController(repositorioEspelhado.Object);


            //Act (a��o)
            var resultado = controller.Login(dados);

            //Assert (verifica��o)
            Assert.IsType<UnauthorizedObjectResult>(resultado); //Vai verificar se o resultado � do tipo objeto n�o autorizado.
        }


        [Fact]
        public void LoginController_Retornar_Token()
        {
            //Arrange
            //A primeira etapa � a cria��o de um objeto do tipo usu�rio.
            Usuario usuarioRetorno = new Usuario();
            usuarioRetorno.Email = "email@email.com";
            usuarioRetorno.Senha = "1234";
            usuarioRetorno.Tipo = "1";
            usuarioRetorno.Id = 1;

            var repositorioEspelhado = new Mock<IUsuarioRepository>();

            repositorioEspelhado.Setup(x => x.Login(It.IsAny<string>(), It.IsAny<string>())).Returns(usuarioRetorno);

            LoginViewModel dados = new LoginViewModel();
            dados.Email = "qualquer string";
            dados.Senha = "qualquer string";

            var controller = new LoginController(repositorioEspelhado.Object);

            string issuerValido = "chapter.webapi";


            //Act
            //� preciso salvar o resultado num tipo objeto para poder acessar os dados dentro dele. Isso � necess�rio pro nosso teste j� que queremos pegar uma parte do token gerado para fazer a compara��o.
            ObjectResult resultado =(ObjectResult) controller.Login(dados);

            string token = resultado.Value.ToString().Split(' ')[3]; //os dados que o objeto resultado receber� ser�o convertidos para string; a informa��o que queremos � a cadeia de caracteres do token, que corresponde � terceira posi��o dessa string, separada pelo m�todo Split em cinco partes.

            //Agora � preciso ler e descriptografar essa sequ�ncia de caracteres. Para isso, � preciso criar uma inst�ncia do objeto JwtSecurityTokenHandler, que possui um m�todo para ler o token.
            var JwtHandler = new JwtSecurityTokenHandler();
            var tokenJwt = JwtHandler.ReadJwtToken(token);


            //Assert
            //O que vamos comparar s�o duas strings: a presente no issuerValido e a presente em tokenJwt.
            Assert.Equal(issuerValido, tokenJwt.Issuer);
        }
    }
}