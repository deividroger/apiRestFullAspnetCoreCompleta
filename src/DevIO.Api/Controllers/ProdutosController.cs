using AutoMapper;
using DevIO.Api.Extensions;
using DevIO.Api.ViewModels;
using DevIO.Business.Interfaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DevIO.Api.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class ProdutosController : MainController
    {
        private readonly IProdutoService _produtoService;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IMapper _mapper;

        public ProdutosController(INotificador notficador, IProdutoService produtoService, IProdutoRepository produtoRepository, IMapper mapper, IUser appUser) : base(notficador, appUser)
        {
            _produtoService = produtoService;
            _produtoRepository = produtoRepository;
            _mapper = mapper;
        }

        [HttpPost]
        [ClaimsAuthorize("Produto", "Adicionar")]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar(ProdutoViewModel produtoViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imagemNome = Guid.NewGuid() + "_" + produtoViewModel.Imagem;

            if (!UploadArquivo(produtoViewModel.ImagemUpload, imagemNome)) return CustomResponse(produtoViewModel);

            produtoViewModel.Imagem = imagemNome;

            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoViewModel));

            return CustomResponse(produtoViewModel);

        }

        [HttpPut("{id:guid}")]
        [ClaimsAuthorize("Produto", "Atualizar")]
        public async Task<IActionResult> Atualizar(Guid id, ProdutoViewModel produtoViewModel)
        {
            if (id != produtoViewModel.Id) 
            {
                NotificarErro("Os ids informados não são iguais");
                return CustomResponse();
            }

            var produtoAtualizacao = await ObterProduto(id);

            produtoViewModel.Imagem = produtoAtualizacao.Imagem;

            if (!ModelState.IsValid) return CustomResponse(ModelState);
 
            if(produtoViewModel.ImagemUpload != null)
            {
                var imagemNome = Guid.NewGuid() + "_" + produtoViewModel.Imagem;

                if(!UploadArquivo(produtoViewModel.ImagemUpload, imagemNome))
                {
                    return CustomResponse(ModelState);
                }

                produtoAtualizacao.Imagem = imagemNome;
            }

            produtoAtualizacao.Nome = produtoViewModel.Nome;
            produtoAtualizacao.Descricao = produtoViewModel.Descricao;
            produtoAtualizacao.Valor = produtoViewModel.Valor;
            produtoAtualizacao.Ativo = produtoViewModel.Ativo;

            await _produtoService.Atualizar(_mapper.Map<Produto>(produtoAtualizacao));

            return CustomResponse(produtoViewModel);
        }

        [HttpPost("Adicionar")]
        [ClaimsAuthorize("Produto", "Adicionar")]

        public async Task<ActionResult<ProdutoViewModel>> AdicionarAlternativo(ProdutoImagemViewModel produtoViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imgPrefixo = Guid.NewGuid() + "_" ;

            if (! await UploadArquivoAlternativo(produtoViewModel.ImagemUpload,imgPrefixo)) return CustomResponse(produtoViewModel);

            produtoViewModel.Imagem = imgPrefixo + produtoViewModel.ImagemUpload.FileName;

            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoViewModel));

            return CustomResponse(produtoViewModel);

        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<ProdutoViewModel>> ObterTodos()
        {
            return _mapper.Map<IEnumerable<ProdutoViewModel>>(await _produtoRepository.ObterProdutosFornecedores());
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProdutoViewModel>> OberPorId(Guid id)
        {
            var produtoViewModel = await ObterProduto(id);

            if (produtoViewModel == null) return NotFound();


            return produtoViewModel;


        }

        [HttpDelete("{id:guid}")]
        [ClaimsAuthorize("Produto", "Remover")]

        public async Task<ActionResult<ProdutoViewModel>> Excluir(Guid id)
        {
            var produto = await ObterProduto(id);

            if (produto == null) return NotFound();

            await _produtoService.Remover(id);

            return CustomResponse(produto);
        }


        private bool UploadArquivo(string arquivo, string imgNome)
        {
            var imageDataByteArray = Convert.FromBase64String(arquivo);

            if (string.IsNullOrEmpty(arquivo))
            {
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/app/demo-webapi/src/assets", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                NotificarErro("Já existe um arquivo com esse nome!");

                return false;
            }

            System.IO.File.WriteAllBytes(filePath, imageDataByteArray);

            return true;

        }

        private async Task<bool> UploadArquivoAlternativo(IFormFile arquivo, string imgPrefixo)
        {
            if(arquivo == null || arquivo.Length == 0)
            {
                NotificarErro("Forneça uma imagem para este produto!");
                return false;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/app/demo-webapi/src/assets", imgPrefixo + arquivo.FileName);

            if (System.IO.File.Exists(filePath))
            {
                NotificarErro("Já existe um arquivo com esse nome!");

                return false;
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            return true;
        }


        private async Task<ProdutoViewModel> ObterProduto(Guid id)
        {
            return _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterProdutosPorFornecedor(id));
        }
    }
}
