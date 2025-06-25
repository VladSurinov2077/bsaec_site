importContent();
function importContent(){
        fetch('./content.html')
        .then(response => response.text())
        .then(html => {
            document.getElementById('contentImportBox').innerHTML = html;
        });
}