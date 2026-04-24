
window.showCartToast = function () {
    const toastEl = document.getElementById('cartToast');
    const toast = new bootstrap.Toast(toastEl);
    toast.show();
}

window.addToCart = function (productId) {
    fetch('/Cart/AddAjax', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `productId=${productId}&quantity=1`
    })
    .then(res => {
        if (!res.ok) throw new Error("Server error");
        return res.json();
    })
    .then(data =>{ 
        updateCartUI(data);
        showCartToast();
    })
    .catch(err => console.error(err));
}

window.updateCartUI = function (data) {

    // badge
    document.querySelectorAll('.total-items')
        .forEach(el => el.innerText = data.count);

    // list
    const list = document.querySelector('.shopping-list');
    list.innerHTML = '';

    data.items.forEach(item => {
        list.innerHTML += `
            <li>
                <a href="javascript:void(0)"
                    class="remove"
                    onclick="removeFromCart(${item.id})">
                    <i class="lni lni-close"></i>
                </a>
                <div class="cart-img-head">
                    <img src="${item.image}" />
                </div>
                <div class="content">
                    <h6>${item.name}</h6>
                    <p>${item.quantity} x ${item.price}</p>
                </div>
            </li>
        `;
    });

    // total
    document.querySelector('.total-amount').innerText = data.total;
}

window.removeFromCart = function (productId) {
    fetch('/Cart/RemoveAjax', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `productId=${productId}`
    })
    .then(res => {
        if (!res.ok) throw new Error("Error");
        return res.json();
    })
    .then(data => updateCartUI(data))
    .catch(err => console.error(err));
}

document.addEventListener("DOMContentLoaded", () => {
    fetch('/Cart/GetCartJson')
        .then(res => {
            if (!res.ok) return null;
            return res.json();
        })
        .then(data => {
            if (data) updateCartUI(data);
        });
});
