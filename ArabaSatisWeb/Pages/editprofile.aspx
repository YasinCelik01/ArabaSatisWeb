<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="editprofile.aspx.cs" Inherits="ArabaSatisWeb.Pages.editprofile" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="../CSS/editprofile.css">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
    <div class="edit-profile-container">
        <div class="edit-profile-card">
            <h2>Profil Düzenle</h2>
            <form id="editProfileForm">
                <div class="form-group">
                    <label for="username">Kullanıcı Adı:</label>
                    <input type="text" id="username" name="username" required>
                </div>
                <div class="form-group">
                    <label for="profileImageURL">Email:</label>
                    <input type="text" id="Email" name="Email">
                </div>
                <button type="submit" class="btn">Profili Güncelle</button>
            </form>
        </div>
    </div>

    <script>

        fetch('/api/kullanici/getprofile', {
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + document.cookie.split('authToken=')[1],
                'Content-Type': 'application/json'
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data && data.Data) {
                document.getElementById('username').value = data.Data.Username;
                document.getElementById('Email').value = data.Data.Email;
            } else {
                alert('Profil bilgileri alınamadı.');
            }
        })
        .catch(error => {
            console.error('Hata:', error);
            alert('Profil yüklenirken bir hata oluştu.');
        });

        // Profil güncelleme işlemi
        document.getElementById('editProfileForm').addEventListener('submit', function(event) {
            event.preventDefault();

            const updatedUser = {
                Username: document.getElementById('username').value,
                Email: document.getElementById('Email').value
            };

            fetch('/api/kullanici/updateprofile', {
                method: 'PUT',
                headers: {
                    'Authorization': 'Bearer ' + document.cookie.split('authToken=')[1],
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(updatedUser)
            })
            .then(response => response.json())
            .then(data => {
                if (data.NewToken) {                    
                    document.cookie = `authToken=${data.NewToken}; path=/`;
                    alert('Profil başarıyla güncellendi. Yeni token oluşturuldu.');
                    window.location.href = "profile.aspx"; // Profil sayfasına yönlendir
                } else if (data.Message) {
                    alert(data.Message);
                } else {
                    alert('Profil güncellenirken bir hata oluştu.');
                }
            })
            .catch(error => {
                console.error('Hata:', error);
                alert('Profil güncellenirken bir hata oluştu.');
            });
        });
    </script>

</asp:Content>
