innerUpperheader();
innerHeader();
innerFooter();
function innerUpperheader() {
    fetch('./upperheader.html')
        .then(response => response.text())
        .then(html => {
            document.getElementById('upperheaderImportBox').innerHTML = html;
        });
}
function innerHeader() {
    fetch('./header.html')
        .then(response => response.text())
        .then(html => {
            document.getElementById('headerImportBox').innerHTML = html;
        });
}
function innerFooter() {
    fetch('./footer.html')
        .then(response => response.text())
        .then(html => {
            document.getElementById('footerImportBox').innerHTML = html;
        });
}