/* ── Tab switch ── */
function switchTab(tab) {
    const tabs = document.querySelectorAll('.auth-tab');
    tabs[0].classList.toggle('active', tab === 'login');
    tabs[1].classList.toggle('active', tab === 'register');

    document.getElementById('panelLogin').classList.toggle('hidden', tab !== 'login');
    document.getElementById('panelRegister').classList.toggle('hidden', tab !== 'register');

    clearAlert();
}

/* ── Password toggle ── */


/* ── Password strength ── */
function checkStrength(val) {
    const wrap = document.getElementById('strengthWrap');
    const bar = document.getElementById('strengthBar');
    const lbl = document.getElementById('strengthLabel');
    if (!val) { wrap.style.display = 'none'; return; }
    wrap.style.display = 'block';

    let score = 0;
    if (val.length >= 8) score++;
    if (val.length >= 12) score++;
    if (/[A-Z]/.test(val)) score++;
    if (/[0-9]/.test(val)) score++;
    if (/[^A-Za-z0-9]/.test(val)) score++;

    const levels = [
        { pct: '20%', color: '#ff6b6b', label: 'Muy débil' },
        { pct: '40%', color: '#ff9e6b', label: 'Débil' },
        { pct: '60%', color: '#ffd166', label: 'Aceptable' },
        { pct: '80%', color: '#4394e0', label: 'Fuerte' },
        { pct: '100%', color: '#6ef98b', label: 'Muy fuerte' },
    ];
    const l = levels[Math.max(0, score - 1)];
    bar.style.width = l.pct;
    bar.style.background = l.color;
    lbl.textContent = l.label;
    lbl.style.color = l.color;
}

/* ── Validation helpers ── */
function setError(fieldId, show) {
    const el = document.getElementById(fieldId);
    if (!el) return;
    el.classList.toggle('has-error', show);
}

function showAlert(type, msg) {
    const el = document.getElementById('authAlert');
    el.className = 'auth-alert ' + type;
    el.innerHTML = (type === 'success'
        ? '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>'
        : '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>'
    ) + ' ' + msg;
    el.style.display = 'flex';
}

function clearAlert() {
    const el = document.getElementById('authAlert');
    el.style.display = 'none';
}

/* ── Login handler ── */


/* ── Register handler ── */



/* ════════════════════════════════════
   INTERNACIONALIZACIÓN
════════════════════════════════════ */
const TRANSLATIONS = {
    es: {
        navBack: 'Volver al inicio',
        tabLogin: 'Iniciar sesión',
        tabRegister: 'Crear cuenta',
        loginTitle: 'Bienvenido de nuevo',
        loginSub: 'Ingresá con tu cuenta para acceder al catálogo completo y realizar compras.',
        fieldEmail: 'Correo electrónico',
        fieldPwd: 'Contraseña',
        fieldPwd2: 'Confirmá la contraseña',
        fieldNombre: 'Nombre',
        fieldApellido: 'Apellido',
        fieldPais: 'País',
        forgotPwd: '¿Olvidaste tu contraseña?',
        btnLogin: 'Iniciar sesión',
        orWith: 'o continuá con',
        noAccount: '¿No tenés cuenta?',
        registerLink: 'Registrate gratis',
        regTitle: 'Creá tu cuenta',
        regSub: 'Unite a la comunidad de surfers de Latinoamérica. Registro gratuito, sin compromiso.',
        btnRegister: 'Crear cuenta gratis',
        hasAccount: '¿Ya tenés cuenta?',
        loginLink: 'Iniciá sesión',
        termsText: 'Acepto los',
        termsLink: 'Términos y Condiciones',
        termsAnd: 'y la',
        privacyLink: 'Política de Privacidad',
        termsBrand: 'de Master Surf.',
        phEmail: 'tu@email.com',
        phPwd: 'Tu contraseña',
        phPwdMin: 'Mínimo 8 caracteres',
        phPwd2: 'Repetí tu contraseña',
        phNombre: 'Nombre',
        phApellido: 'Apellido',
        phPais: 'Argentina, Brasil, Uruguay...',
        alertLoading: 'Ingresando...',
        alertWelcome: '¡Bienvenido de nuevo, {name}! Redirigiendo...',
        alertBadCreds: 'Email o contraseña incorrectos. Verificá tus datos.',
        alertRegLoad: 'Creando cuenta...',
        alertRegDone: '¡Cuenta creada con éxito! Bienvenido/a, {name}. Redirigiendo...',
        alertDupEmail: 'Ese email ya está registrado. ¿Querés iniciar sesión?',
        alertTerms: 'Tenés que aceptar los Términos y Condiciones para continuar.',
    },
    en: {
        navBack: 'Back to home',
        tabLogin: 'Sign in',
        tabRegister: 'Create account',
        loginTitle: 'Welcome back',
        loginSub: 'Sign in to access the full catalog and place orders.',
        fieldEmail: 'Email address',
        fieldPwd: 'Password',
        fieldPwd2: 'Confirm password',
        fieldNombre: 'First name',
        fieldApellido: 'Last name',
        fieldPais: 'Country',
        forgotPwd: 'Forgot your password?',
        btnLogin: 'Sign in',
        orWith: 'or continue with',
        noAccount: "Don't have an account?",
        registerLink: 'Sign up for free',
        regTitle: 'Create your account',
        regSub: 'Join the Latin American surf community. Free registration, no commitment.',
        btnRegister: 'Create free account',
        hasAccount: 'Already have an account?',
        loginLink: 'Sign in',
        termsText: 'I agree to the',
        termsLink: 'Terms and Conditions',
        termsAnd: 'and the',
        privacyLink: 'Privacy Policy',
        termsBrand: 'of Master Surf.',
        phEmail: 'you@email.com',
        phPwd: 'Your password',
        phPwdMin: 'At least 8 characters',
        phPwd2: 'Repeat your password',
        phNombre: 'First name',
        phApellido: 'Last name',
        phPais: 'Argentina, Brazil, Uruguay...',
        alertLoading: 'Signing in...',
        alertWelcome: 'Welcome back, {name}! Redirecting...',
        alertBadCreds: 'Incorrect email or password. Please check your details.',
        alertRegLoad: 'Creating account...',
        alertRegDone: 'Account created! Welcome, {name}. Redirecting...',
        alertDupEmail: 'That email is already registered. Want to sign in?',
        alertTerms: 'You must accept the Terms and Conditions to continue.',
    },
    pt: {
        navBack: 'Voltar ao início',
        tabLogin: 'Entrar',
        tabRegister: 'Criar conta',
        loginTitle: 'Bem-vindo de volta',
        loginSub: 'Entre com sua conta para acessar o catálogo completo e fazer compras.',
        fieldEmail: 'Endereço de e-mail',
        fieldPwd: 'Senha',
        fieldPwd2: 'Confirmar senha',
        fieldNombre: 'Nome',
        fieldApellido: 'Sobrenome',
        fieldPais: 'País',
        forgotPwd: 'Esqueceu sua senha?',
        btnLogin: 'Entrar',
        orWith: 'ou continue com',
        noAccount: 'Não tem uma conta?',
        registerLink: 'Cadastre-se grátis',
        regTitle: 'Crie sua conta',
        regSub: 'Junte-se à comunidade de surfistas da América Latina. Cadastro gratuito, sem compromisso.',
        btnRegister: 'Criar conta grátis',
        hasAccount: 'Já tem uma conta?',
        loginLink: 'Entrar',
        termsText: 'Aceito os',
        termsLink: 'Termos e Condições',
        termsAnd: 'e a',
        privacyLink: 'Política de Privacidade',
        termsBrand: 'da Master Surf.',
        phEmail: 'voce@email.com',
        phPwd: 'Sua senha',
        phPwdMin: 'Mínimo 8 caracteres',
        phPwd2: 'Repita sua senha',
        phNombre: 'Nome',
        phApellido: 'Sobrenome',
        phPais: 'Argentina, Brasil, Uruguai...',
        alertLoading: 'Entrando...',
        alertWelcome: 'Bem-vindo de volta, {name}! Redirecionando...',
        alertBadCreds: 'E-mail ou senha incorretos. Verifique seus dados.',
        alertRegLoad: 'Criando conta...',
        alertRegDone: 'Conta criada com sucesso! Bem-vindo/a, {name}. Redirecionando...',
        alertDupEmail: 'Esse e-mail já está registrado. Quer entrar?',
        alertTerms: 'Você precisa aceitar os Termos e Condições para continuar.',
    },
};

let currentLang = localStorage.getItem('zsg_lang') || 'es';

function t(key, vars) {
    let str = (TRANSLATIONS[currentLang] || TRANSLATIONS.es)[key] || key;
    if (vars) Object.keys(vars).forEach(k => str = str.replace('{' + k + '}', vars[k]));
    return str;
}

function applyLang() {
    // Text nodes
    document.querySelectorAll('[data-i18n]').forEach(el => {
        el.textContent = t(el.dataset.i18n);
    });
    // Placeholders
    document.querySelectorAll('[data-i18n-ph]').forEach(el => {
        el.placeholder = t(el.dataset.i18nPh);
    });
    // html lang attribute
    document.documentElement.lang = currentLang;
    // Active button
    document.querySelectorAll('.lang-btn').forEach(btn => {
        btn.classList.toggle('active', btn.textContent.trim().toLowerCase() === currentLang);
    });
}

function setLang(lang) {
    currentLang = lang;
    localStorage.setItem('zsg_lang', lang);
    // Update dropdown UI
    const flag = document.getElementById('langFlag');
    const code = document.getElementById('langCode');
    if (flag) flag.textContent = LANG_FLAGS[lang];
    if (code) code.textContent = LANG_CODES[lang];
    document.querySelectorAll('.lang-option').forEach(btn => {
        btn.classList.toggle('active', btn.dataset.lang === lang);
    });
    document.getElementById('langSwitcher')?.classList.remove('open');
    document.documentElement.lang = lang;
    applyLang();
}

function toggleLangDropdown() {
    document.getElementById('langSwitcher').classList.toggle('open');
}
document.addEventListener('click', e => {
    if (!document.getElementById('langSwitcher')?.contains(e.target)) {
        document.getElementById('langSwitcher')?.classList.remove('open');
    }
});

const LANG_FLAGS = { es: '\u{1F1FA}\u{1F1FE}', en: '\u{1F1EC}\u{1F1E7}', pt: '\u{1F1E7}\u{1F1F7}' };
const LANG_CODES = { es: 'ES', en: 'EN', pt: 'PT' };

// Init switcher display
(function () {
    const flag = document.getElementById('langFlag');
    const code = document.getElementById('langCode');
    if (flag) flag.textContent = LANG_FLAGS[currentLang];
    if (code) code.textContent = LANG_CODES[currentLang];
    document.querySelectorAll('.lang-option').forEach(btn => {
        btn.classList.toggle('active', btn.dataset.lang === currentLang);
    });
})();

// Apply on load
applyLang();

document.addEventListener('keydown', function (event) {
    if (event.key !== 'Enter') return;

    const elementoActivo = document.activeElement;

    if (
        elementoActivo &&
        elementoActivo.tagName === 'TEXTAREA'
    ) {
        return;
    }

    const panelRegistro =
        document.getElementById('panelRegister');

    const panelLogin =
        document.getElementById('panelLogin');

    if (
        panelRegistro &&
        !panelRegistro.classList.contains('hidden')
    ) {
        panelRegistro
            .querySelector('form')
            ?.requestSubmit();

        return;
    }

    if (
        panelLogin &&
        !panelLogin.classList.contains('hidden')
    ) {
        panelLogin
            .querySelector('form')
            ?.requestSubmit();
    }
});

function togglePwd(inputId, button) {
    const input = document.getElementById(inputId);

    if (!input) return;

    const icon = button.querySelector("svg");

    if (input.type === "password") {
        input.type = "text";

        if (icon) {
            icon.innerHTML = `
                <path d="M17.94 17.94A10.94 10.94 0 0112 20C5 20 1 12 1 12a21.8 21.8 0 015.06-6.94"/>
                <path d="M9.9 4.24A10.77 10.77 0 0112 4c7 0 11 8 11 8a21.3 21.3 0 01-3.22 4.91"/>
                <line x1="1" y1="1" x2="23" y2="23"/>
            `;
        }

        button.title = "Ocultar contraseña";
    } else {
        input.type = "password";

        if (icon) {
            icon.innerHTML = `
                <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
                <circle cx="12" cy="12" r="3"/>
            `;
        }

        button.title = "Ver contraseña";
    }
}