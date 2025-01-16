<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="ArabaSatisWeb.Pages.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="../CSS/Login.css">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="wrapper">
        <form id="loginForm" onsubmit="loginUser(event)">
            <h2>Login</h2>
            <div class="input-field">
                <input type="text" id="email" required>
                <label>Enter your email</label>
            </div>
            <div class="input-field">
                <input type="password" id="password" required>
                <label>Enter your password</label>
            </div>
            <button type="submit">Log In</button>
            <div class="register">
                <p>Don't have an account? <a href="Register.aspx">Register</a></p>
            </div>
        </form>

       
        <div id="resultMessage" class="message"></div>
    </div>

    <script>
        
        async function loginUser(event) {
            event.preventDefault();

            const Email = document.getElementById('email').value;
            const PasswordHash = document.getElementById('password').value;

            const loginData = {
                email: Email,
                passwordHash: PasswordHash
            };

            try {
                const response = await fetch('https://localhost:44381/api/kullanici/login', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(loginData)
                });

                const resultMessageElement = document.getElementById('resultMessage');
                resultMessageElement.style.display = 'block';

                if (!response.ok) {
                    const errorData = await response.json(); 
                    resultMessageElement.innerText = errorData.Message; 
                    resultMessageElement.className = 'message error'; 
                } else {
                    const successData = await response.json(); 
                    resultMessageElement.innerText = successData.Message; 
                    resultMessageElement.className = 'message success';
                    
                    document.cookie = `authToken=${successData.Data.Token}; path=/; Secure;`;
                                       
                    setTimeout(() => {
                        window.location.href = "anasayfa.aspx";
                    }, 1000);
                }
            } catch (error) {
                const resultMessageElement = document.getElementById('resultMessage');
                resultMessageElement.style.display = 'block';
                resultMessageElement.innerText = 'Login sırasında bir hata oluştu: ' + error.message;
                resultMessageElement.className = 'message error';
            }
        }

    </script>
</asp:Content>
