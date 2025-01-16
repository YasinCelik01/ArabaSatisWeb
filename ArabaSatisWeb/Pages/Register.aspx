<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="ArabaSatisWeb.Pages.Register" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="../CSS/Login.css">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="wrapper">
        <form id="registerForm" onsubmit="registerUser(event)">
            <h2>Register</h2>
            <div class="input-field">
                <input type="text" id="username" required>
                <label>Enter your username</label>
            </div>
            <div class="input-field">
                <input type="password" id="password" required>
                <label>Enter your password</label>
            </div>
            <div class="input-field">
                <input type="email" id="email" required>
                <label>Enter your email</label>
            </div>
            <button type="submit">Register</button>
            <div class="login">
                <p>Already have an account? <a href="Login.aspx">Login</a></p>
            </div>
        </form>
        <div id="resultMessage" style="display: none;"></div>       
    </div>

    <script>
        async function registerUser(event) {

            event.preventDefault();
            
            const username = document.getElementById('username').value;
            const password = document.getElementById('password').value;
            const email = document.getElementById('email').value;
            
            const userData = {
                Username: username,
                PasswordHash: password,
                Email: email
            };

            try {
                
                const response = await fetch('https://localhost:44381/api/kullanici/register', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(userData),
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

                    setTimeout(() => {
                        window.location.href = "Login.aspx";
                    }, 2000);
                }
            } catch (error) {
                const resultMessageElement = document.getElementById('resultMessage');
                resultMessageElement.style.display = 'block';
                resultMessageElement.innerText = 'Kayıt sırasında bir hata oluştu: ' + error;
                resultMessageElement.className = 'message error';
            }
        }
    </script>

</asp:Content>
