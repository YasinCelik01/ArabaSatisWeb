<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="admin.aspx.cs" Inherits="ArabaSatisWeb.Pages.admin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="../CSS/AdminPanel.css">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
   
    <div class="admin-panel-container">
        <div class="admin-panel-card">
            <h1>Admin Paneli</h1>
            <p>Admin paneline başarılı şekilde giriş yaptınız. Burada admin özelliklerine erişebilirsiniz.</p>
            <button onclick="goToAdminDashboard()" class="btn btn-dashboard">Admin Dashboard</button>
        </div>
    </div>

    <script>
        // Admin paneli için JWT token kontrolü
        fetch('/api/admin/panel', {
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + document.cookie.split('authToken=')[1],
                'Content-Type': 'application/json'
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data.Message === "Admin paneline başarılı şekilde erişildi.") {
                    // Admin paneli içeriğini burada gösterebiliriz
                    console.log("Admin paneline başarıyla erişildi.");
                } else {
                    alert(data.Message); // Kullanıcı admin değilse mesajı göster
                    window.location.href = "anasayfa.aspx"; // Ana sayfaya yönlendir
                }
            })
            .catch(error => {
                console.error('Hata:', error);
                window.location.href = "login.aspx"; // Hata durumunda login sayfasına yönlendir
            });

        function goToAdminDashboard() {
            // Admin paneli yönetim sayfasına yönlendirme
            window.location.href = "dashboard.aspx"; // Admin paneli dashboard URL'si
        }

        
    </script>

</asp:Content>
