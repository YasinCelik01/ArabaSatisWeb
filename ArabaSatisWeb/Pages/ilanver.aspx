<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ilanver.aspx.cs" Inherits="ArabaSatisWeb.Pages.ilanver" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="../CSS/ilan.css">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container">
        <!-- İlan Verme Formu -->
        <div class="form-container">
            <h1>Araba İlanı Ver</h1>
            <form id="carForm" enctype="multipart/form-data">
                <label for="marka">Marka:</label>
                <input type="text" id="marka" name="marka" required>

                <label for="model">Model:</label>
                <input type="text" id="model" name="model" required>

                <label for="yil">Yıl:</label>
                <input type="number" id="yil" name="yil" required>

                <label for="fiyat">Fiyat:</label>
                <input type="number" id="fiyat" name="fiyat" required>

                <label for="kilometre">Kilometre:</label>
                <input type="number" id="kilometre" name="kilometre" required>

                <label for="yakitTuru">Yakıt Türü:</label>
                <select id="yakitTuru" name="yakitTuru" required>
                    <option value="Benzin">Benzin</option>
                    <option value="Dizel">Dizel</option>
                    <option value="LPG">LPG</option>
                </select>

                <label for="vitesTuru">Vites Türü:</label>
                <select id="vitesTuru" name="vitesTuru" required>
                    <option value="Manuel">Manuel</option>
                    <option value="Otomatik">Otomatik</option>
                </select>

                <label for="kasaTuru">Kasa Türü:</label>
                <input type="text" id="kasaTuru" name="kasaTuru" required>

                <label for="motorHacmi">Motor Hacmi:</label>
                <input id="motorHacmi" name="motorHacmi" required>

                <label for="renk">Renk:</label>
                <input type="text" id="renk" name="renk" required>

                <label for="Telefon">Telefon:</label>
                <input type="number" id="Telefon" name="Telefon" required>

                <label for="aciklama">Açıklama:</label>
                <textarea id="aciklama" name="aciklama"></textarea>

                <label for="foto">Fotoğraflar:</label>
                <input type="file" id="foto" name="foto" accept="image/*" multiple>

                <button type="submit">İlanı Ver</button>
            </form>
        </div>
    </div>

    <script>
        // İlan verme formu işlemi
        document.getElementById('carForm').addEventListener('submit', function (event) {
            event.preventDefault();

            const formData = new FormData(this);
            const carData = {
                marka: formData.get('marka'),
                model: formData.get('model'),
                yil: formData.get('yil'),
                fiyat: formData.get('fiyat'),
                kilometre: formData.get('kilometre'),
                yakitTuru: formData.get('yakitTuru'),
                vitesTuru: formData.get('vitesTuru'),
                kasaTuru: formData.get('kasaTuru'),
                motorHacmi: formData.get('motorHacmi'),
                renk: formData.get('renk'),
                Telefon: formData.get('Telefon'),
                aciklama: formData.get('aciklama'),
                fotografListesi: []
            };

            const files = formData.getAll('foto');

            // Fotoğrafları yükleme işlemini Promise ile yönetiyoruz
            const filePromises = files.map(file => {
                return new Promise((resolve, reject) => {
                    const reader = new FileReader();
                    reader.onloadend = function () {
                        resolve({
                            Fotograf: reader.result.split(',')[1], // Base64 encoded image
                            VarsayilanFotograf: false, // Varsayılan fotoğrafı seçmek için logic ekleyebilirsiniz
                            KayitTarihi: new Date().toISOString()
                        });
                    };
                    reader.onerror = reject;
                    reader.readAsDataURL(file);
                });
            });

            // Tüm fotoğraflar yüklendikten sonra API'ye veri gönderiyoruz
            Promise.all(filePromises)
                .then(fotografListesi => {
                    carData.fotografListesi = fotografListesi;

                    // API'ye POST isteği gönderme
                    return fetch('/api/araba/ekle', {
                        method: 'POST',
                        headers: {
                            'Authorization': 'Bearer ' + document.cookie.split('authToken=')[1],
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(carData)
                    });
                })
                .then(response => response.json())
                .then(data => {
                    alert(data.Message);
                    // Başarılı ise profile sayfasına yönlendir
                    if (data.Message === "Araba başarıyla eklendi.") {
                        window.location.href = 'profile.aspx'; // Profile sayfasına yönlendirme
                    }
                })
                .catch(error => {
                    alert("Bir hata oluştu!");
                    console.error(error);
                });
        });
</script>
</asp:Content>
