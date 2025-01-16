<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="deneme.aspx.cs" Inherits="ArabaSatisWeb.deneme" %>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="wrapper">
        <form action="#">
            <h2>Login</h2>
            <div class="input-field">
                <input type="text" id="loginUsername" required>
                <label>Enter your email</label>
            </div>
            <div class="input-field">
                <input type="password" id="loginPassword" required>
                <label>Enter your password</label>
            </div>
            <div class="forget">
                <label for="remember">
                    <input type="checkbox" id="remember">
                    <p>Remember me</p>
                </label>
                <a href="#">Forgot password?</a>
            </div>
            <button type="button" onclick="loginUser()">Log In</button>
            <div class="register">
                <p>Don't have an account? <a href="#">Register</a></p>
            </div>
        </form>
    </div>

    <script>
        function registerUser() {
            const data = {
                Username: document.getElementById("username").value,
                Email: document.getElementById("email").value,
                PasswordHash: document.getElementById("password").value,
                Role: "Müşteri"
            };
            fetch('/api/kullanici/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            })
                .then(response => response.json())
                .then(data => alert(data.message))
                .catch(error => console.error('Error:', error));
        }

        function loginUser() {
            const data = {
                Username: document.getElementById("loginUsername").value,
                PasswordHash: document.getElementById("loginPassword").value
            };
            fetch('/api/kullanici/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            })
                .then(response => response.json())
                .then(data => alert(data.message))
                .catch(error => console.error('Error:', error));
        }
    </script>
</asp:Content>
