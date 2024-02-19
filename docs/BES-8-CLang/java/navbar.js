function loadHTML()
{
    fetch('/BES-8-CLang/navbar.html')
    .then(response=> response.text())
    .then(text=> document.getElementById('navbar').innerHTML = text);
}