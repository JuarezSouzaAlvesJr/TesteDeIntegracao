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
            //Arrange (preparação). Primeiro, precisamos espelhar o repositório (não iremos usar o original, pois não iremos fazer a conexão direta com o banco de dados) e configurar o método login para o que desejamos receber.
            var repositorioEspelhado = new Mock<IUsuarioRepository>();

            //através do método 'Setup' acessamos o método login do repositório original. Como iremos configurar o retorno do método login, não importa o argumento email e senha, por isso usamos "It.IsAny<string>()" (isso é qualquer string). Duas vezes porque o método login solicita duas strings (email e senha).
            repositorioEspelhado.Setup(x => x.Login(It.IsAny<string>(), It.IsAny<string>())).Returns((Usuario)null); //O último método é para obter como retorno um usuário nulo.

            //Quando chamamos o método login, é esperado receber um objeto do tipo LoginViewModel com os dados que irá buscar no banco. Então, precisamos criar um objeto desse tipo.
            LoginViewModel dados = new LoginViewModel();
            dados.Email = "email_inexistente@email.com";
            dados.Senha = "09876";

            //Agora, é preciso criar uma instância do controller para chamar o loginController
            var controller = new LoginController(repositorioEspelhado.Object);


            //Act (ação)
            var resultado = controller.Login(dados);

            //Assert (verificação)
            Assert.IsType<UnauthorizedObjectResult>(resultado); //Vai verificar se o resultado é do tipo objeto não autorizado.
        }


        [Fact]
        public void LoginController_Retornar_Token()
        {
            //Arrange
            //A primeira etapa é a criação de um objeto do tipo usuário.
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
            //É preciso salvar o resultado num tipo objeto para poder acessar os dados dentro dele. Isso é necessário pro nosso teste já que queremos pegar uma parte do token gerado para fazer a comparação.
            ObjectResult resultado =(ObjectResult) controller.Login(dados);

            string token = resultado.Value.ToString().Split(' ')[3]; //os dados que o objeto resultado receberá serão convertidos para string; a informação que queremos é a cadeia de caracteres do token, que corresponde à terceira posição dessa string, separada pelo método Split em cinco partes.

            //Agora é preciso ler e descriptografar essa sequência de caracteres. Para isso, é preciso criar uma instância do objeto JwtSecurityTokenHandler, que possui um método para ler o token.
            var JwtHandler = new JwtSecurityTokenHandler();
            var tokenJwt = JwtHandler.ReadJwtToken(token);


            //Assert
            //O que vamos comparar são duas strings: a presente no issuerValido e a presente em tokenJwt.
            Assert.Equal(issuerValido, tokenJwt.Issuer);
        }
    }
}