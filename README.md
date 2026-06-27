<div align="center">
  <img src="./assets/logo.svg" alt="LaPDF Logo" width="40%" /> 
  </br> </br>
  
  [![GitHub](https://img.shields.io/badge/github-LaPDF-181717?style=for-the-badge&logo=github)](https://github.com/seu-usuario/lapdf)
  [![Docs](https://img.shields.io/badge/docs-guide-239120?style=for-the-badge&logo=gitbook&logoColor=white)](docs/Setup.md)
  [![License](https://img.shields.io/badge/license-MIT-blue?style=for-the-badge)](LICENSE)
  [![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)

</div>

---

## **📖 SOBRE O PROJETO**

> O **LaPDF** é uma ferramenta web gratuita e intuitiva para manipulação de arquivos PDF. Com uma interface simples e moderna, oferece todas as funcionalidades essenciais para trabalhar com PDFs sem necessidade de login ou instalação.

### **✨ Funcionalidades:**
```markdown
✅ Compressão de PDF com diferentes níveis de qualidade
✅ Mesclagem de múltiplos arquivos PDF
✅ Divisão de PDF em páginas individuais
✅ Conversão entre formatos (PDF ↔ Word, Excel, PowerPoint)
✅ Edição básica de PDF (texto, imagens, anotações)
✅ Proteção com senha e remoção de segurança
✅ 100% GRÁTIS e sem necessidade de cadastro
✅ Interface responsiva e intuitiva
```
### **🔧 Fluxo de Dados**
> Upload do PDF → Processamento (compressão/mesclagem/divisão) → Download do resultado

---

## 🛠️ TECH STACK

| Categoria | Tecnologias |
|-----------|-------------|
| **Languages** | ![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white) |
| **Frontend** | ![Razor Pages](https://img.shields.io/badge/Razor_Pages-512BD4?style=flat-square&logo=dotnet&logoColor=white) ![HTML5](https://img.shields.io/badge/HTML5-E34F26?style=flat-square&logo=html5&logoColor=white) ![CSS3](https://img.shields.io/badge/CSS3-1572B6?style=flat-square&logo=css3&logoColor=white) ![JavaScript](https://img.shields.io/badge/JavaScript-F7DF1E?style=flat-square&logo=javascript&logoColor=black) |
| **PDF Processing** | ![iTextSharp](https://img.shields.io/badge/iTextSharp-00A8E8?style=flat-square&logo=pdf&logoColor=white) |

---

## **📸 DEMO**
<div align="center"> <img src="./docs/assets/demo.svg" alt="KwanzaSmart Dashboard" width="65%" /> <br /> <i>Interface principal com gráficos de temperatura, pH e alertas em tempo real</i> </div>


---

## 🚀 COMO EXECUTAR

### Pré-requisitos

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Passos para execução

```bash
# 1. Clonar o repositório
git clone https://github.com/seu-usuario/la-pdf.git
cd la-pdf

# 2. Restaurar pacotes
dotnet restore src/LaPDF.Web

# 3. Executar o projeto
dotnet run --project src/LaPDF.Web/LaPDF.Web.csproj

# 4. Acessar no navegador
# https://localhost:5001
```

### Executar os testes
```bash
dotnet test src/LaPDF.UnitTests/LaPDF.UnitTests.csproj
``` 

---
## 📂 ESTRUTURA DO PROJETO
```text
LaPDF/
├── src/
│   ├── LaPDF.Web/              # Aplicação principal com interface web
│   └── LaPDF.UnitTests/        # Testes unitários da aplicação
├── assets/                     # Imagens, ícones e recursos visuais do README
├── docs/                       # Documentação adicional do projeto
├── LaPDF.slnx                  # Arquivo de solução do Visual Studio
├── LICENSE                     # Licença MIT do projeto
├── .gitignore                  # Arquivos ignorados pelo Git
└── README.md                   # Documentação principal do projeto
```

---

## **📄 LICENÇA**

> Este projeto está sob a licença MIT, o que significa que é de código aberto e pode ser utilizado livremente para fins académicos e comerciais, desde que mantidos os créditos.

```markdown
📚 Código aberto (open source)
✅ Livre para uso académico
🤝 Contribuições são bem-vindas
```