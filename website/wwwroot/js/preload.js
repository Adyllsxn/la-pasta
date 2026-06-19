'use strict';

const preloader = document.querySelector("[data-preaload]");

// Função para esconder o preloader com animação
function hidePreloader() {
    if (preloader) {
        preloader.classList.add("loaded");
        document.body.classList.add("loaded");
        
        // Remove o preloader do DOM após a animação
        setTimeout(() => {
            if (preloader.parentNode) {
                preloader.style.display = 'none';
            }
        }, 800);
    }
}

// Função para verificar se o Blazor carregou
function waitForBlazor() {
    // Verifica se o Blazor já está disponível
    if (typeof Blazor !== 'undefined' && Blazor._internal) {
        console.log('✅ Blazor carregado!');
        setTimeout(hidePreloader, 400);
        return;
    }
    
    // Observa mudanças no DOM para detectar o carregamento do Blazor
    const observer = new MutationObserver(() => {
        const app = document.getElementById('app');
        if (app) {
            // Verifica se o conteúdo do Blazor está presente
            const hasContent = app.querySelector('main') || 
                              app.querySelector('header') ||
                              app.querySelector('.page') ||
                              app.children.length > 1;
            
            if (hasContent) {
                console.log('✅ Conteúdo Blazor detectado!');
                observer.disconnect();
                setTimeout(hidePreloader, 400);
            }
        }
    });
    
    observer.observe(document.body, { 
        childList: true, 
        subtree: true 
    });
}

// Inicia a verificação
window.addEventListener("load", function() {
    console.log('📦 Aguardando carregamento do Blazor...');
    
    // Espera o Blazor carregar
    waitForBlazor();
    
    // Fallback: se demorar mais de 10 segundos, força esconder
    setTimeout(() => {
        console.log('⏰ Fallback: escondendo preloader');
        hidePreloader();
    }, 10000);
});

// Quando o Blazor terminar de carregar (evento específico)
document.addEventListener('blazor-load', function() {
    console.log('✅ Evento blazor-load disparado!');
    setTimeout(hidePreloader, 300);
});