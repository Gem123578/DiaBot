let selectedDiamond = null;
let currentOrder = null;
function selectCard(card, item) {

    document.querySelectorAll(".card").forEach(x => {
        x.classList.remove("selected");
    });

    card.classList.add("selected");

    selectedDiamond = item;

    const purchaseBar =
        document.getElementById("purchaseBar");

    const selectedText =
        document.getElementById("selectedText");

    let text = "";

    // Special Package
    if (item.type === "special") {

        text =
            `${ item.name } • ${ Number(item.price).toLocaleString() } Ks`;
    }

    // Recharge Event
    else if (item.type === "recharge") {

        text =
            `${ item.diamond } + ${ item.bonus } Diamond • ${ Number(item.price).toLocaleString() } Ks`;
    }

    // Normal Diamond
    else {

        text =
            `${ Number(item.diamond).toLocaleString() } Diamond • ${ Number(item.sellingPrice).toLocaleString() } Ks`;
    }

    selectedText.innerText = text;

    purchaseBar.style.display = "flex";

    purchaseBar.animate(
        [
            {
                opacity: 0,
                transform: "translateY(15px)"
            },
            {
                opacity: 1,
                transform: "translateY(0)"
            }
        ],
        {
            duration: 350,
            easing: "cubic-bezier(.2,.8,.2,1)"
        }
    );

    console.log("Selected:", selectedDiamond);
}

document.addEventListener("DOMContentLoaded", function () {

    console.log("Diamond Selling page loaded.");

    loadDiamondPrices();

});

function loadSpecialPackages(data) {

    const grid =
        document.getElementById("specialGrid");

    if (!grid) {
        return;
    }

    grid.innerHTML = "";

    if (
        !data.specialPackages ||
        data.specialPackages.length === 0
    ) {

        grid.innerHTML = `
    <div class="loading" >
        အထူး Package မရှိသေးပါ။
            </div >
    `;

        return;
    }

    data.specialPackages.forEach(
        (item, index) => {

            const card =
                document.createElement("div");

            card.className =
                "card";

            card.style.animationDelay =
                `${ index * 100 } ms`;


            // ICON
            let icon = "🎁";

            if (item.type === "weekly") {
                icon = "⚡";
            }
            else if (item.type === "monthly") {
                icon = "👑";
            }
            else if (item.type === "elite") {
                icon = "🔥";
            }


            // CARD HTML
            card.innerHTML = `
    <div class="diamond-icon" >
        ${ icon }
                </div >

                <div class="diamond-count">
                    ${item.name}
                </div>

                <div class="diamond-label">
                    အထူး Package
                </div>

                <div class="price">
                    ${Number(item.sellingPrice).toLocaleString()} Ks
                </div>
`;


            card.addEventListener("click", function () { selectCard(card, { type: "special", packageType: item.type, name: item.name, price: item.sellingPrice }); });


            grid.appendChild(card);

        }
    );
}



function loadRechargePackages(data) {

    const grid =
        document.getElementById("rechargeGrid");

    if (!grid) {
        return;
    }

    grid.innerHTML = "";

    if (
        !data.rechargeEvent ||
        data.rechargeEvent.length === 0
    ) {

        grid.innerHTML = `
    <div class="loading" >
        Recharge Event မရှိသေးပါ။
            </div >
    `;

        return;
    }

    data.rechargeEvent.forEach(
        (item, index) => {

            const card =
                document.createElement("div");

            card.className =
                "card";

            card.style.animationDelay =
                `${ index * 100 } ms`;


            // CARD HTML
            card.innerHTML = `
    <div class="diamond-icon" >
                    🔥
                </div >

                <div class="diamond-count">
                    ${item.diamond} + ${item.bonus}
                </div>

                <div class="diamond-label">
                    ${item.total} Diamond Total
                </div>

                <div class="price">
                    ${Number(item.sellingPrice).toLocaleString()} Ks
                </div>
`;

            card.addEventListener("click", function () {

                selectCard(card, {
                    type: "recharge",
                    packageType: "recharge",
                    name: item.name,
                    diamond: item.diamond,
                    bonus: item.bonus,
                    total: item.total,
                    price: item.sellingPrice
                });

            });

            grid.appendChild(card);

        }
    );
}



function loadDiamondPackages(data) {

    const grid =
        document.getElementById("diamondGrid");

    if (!grid) {
        return;
    }

    grid.innerHTML = "";

    if (
        !data.prices ||
        data.prices.length === 0
    ) {

        grid.innerHTML = `
    <div class="loading" >
        Diamond Package မရှိသေးပါ။
            </div >
    `;

        return;
    }

    data.prices.forEach(
        (item, index) => {

            const card =
                document.createElement("div");

            card.className =
                "card";

            card.style.animationDelay =
                `${ index * 70 } ms`;


            // CARD HTML
            card.innerHTML = `
    <div class="diamond-icon" >
                    💎
                </div >

                <div class="diamond-count">
                    ${Number(item.diamond).toLocaleString()}
                </div>

                <div class="diamond-label">
                    Diamond
                </div>

                <div class="price">
                    ${Number(item.sellingPrice).toLocaleString()} Ks
                </div>
`;


            // CLICK
            card.addEventListener(
                "click",
                function () {

                    selectCard(
                        card,
                        {
                            type: "diamond",
                            ...item
                        }
                    );

                }
            );


            grid.appendChild(card);

        }
    );
}



async function loadDiamondPrices() {

    const loading =
        document.getElementById("loading");

    const error =
        document.getElementById("error");

    try {

        loading.style.display = "block";

        error.innerText = "";


        // API
        const response =
            await fetch(
                "/Telegram/GetPinnedMessage"
            );


        if (!response.ok) {

            throw new Error(
                "Diamond ဈေးနှုန်း ရယူ၍မရပါ။"
            );

        }


        const data =
            await response.json();


        console.log(
            "API Response:",
            data
        );


        if (!data.success) {

            throw new Error(
                data.message ||
                "ဈေးနှုန်း ရယူ၍မရပါ။"
            );

        }


        // LOAD SPECIAL
        loadSpecialPackages(data);


        // LOAD RECHARGE
        loadRechargePackages(data);


        // LOAD NORMAL DIAMONDS
        loadDiamondPackages(data);

    }
    catch (err) {

        console.error(err);

        error.innerText =
            err.message ||
            "တစ်ခုခုမှားယွင်းနေပါသည်။";

    }
    finally {

        loading.style.display = "none";

    }
}



document
    .getElementById("submitOrderButton")
    .addEventListener("click", async function () {

        // ==========================================
        // Check Current Order
        // ==========================================

        if (!window.currentOrder) {

            showAppAlert({
                title: "Error",
                message: "Order information မတွေ့ပါ။",
                type: "error"
            });

            return;
        }


        // ==========================================
        // Check Screenshot
        // ==========================================

        const fileInput =
            document.getElementById("paymentScreenshot");

        const file =
            fileInput.files[0];

        if (!file) {

            showAppAlert({
                title: "Warning",
                message: "Payment Screenshot တင်ပေးပါ။",
                type: "warning"
            });

            return;
        }


        // ==========================================
        // Button Loading
        // ==========================================

        const button = this;

        const oldText =
            button.innerHTML;

        button.disabled = true;

        button.innerHTML =
            "⏳ Order ပို့နေပါသည်...";


        try {

            // ==========================================
            // FormData
            // ==========================================

            const formData =
                new FormData();

            formData.append(
                "UserId",
                window.currentOrder.userId
            );

         

            formData.append(
                "ServerId",
                window.currentOrder.serverId
            );

            

            formData.append(
                "PackageName",
                window.currentOrder.packageName
            );

            formData.append(
                "Diamond",
                window.currentOrder.diamond
            );

            formData.append(
                "SellingPrice",
                window.currentOrder.sellingPrice
            );

            if (!selectedPaymentMethod) {

                showAppAlert({
                    title: "Warning",
                    message: "KPay သို့မဟုတ် WavePay ကို ရွေးပေးပါ။",
                    type: "warning"
                });

                return;

            }

            // Send Payment Method
            formData.append(
                "PaymentMethod",
                selectedPaymentMethod
            );

            formData.append(
                "paymentScreenshot",
                file
            );


            // ==========================================
            // Submit
            // ==========================================

            const response =
                await fetch(
                    "/Telegram/SubmitOrder",
                    {
                        method: "POST",
                        body: formData
                    }
                );


            const result =
                await response.json();


            if (!response.ok ||
                !result.success) {

                throw new Error(
                    result.message ||
                    "Order ပို့၍မရပါ။"
                );
            }


            // ==========================================
            // Close Modal
            // ==========================================

            closePaymentModal();


            // ==========================================
            // Success
            // ==========================================

            showAppAlert({
                title: "Order Submitted",
                message:
                    "Order နှင့် Payment Screenshot ကို Seller ဆီသို့ ပို့ပြီးပါပြီ။ ကျေးဇူးတင်ပါတယ်။",
                type: "success"
            });


            // ==========================================
            // Reset Screenshot
            // ==========================================

            fileInput.value = "";

            document
                .getElementById("uploadPreview")
                .innerHTML = `
                    <span class="upload-icon">
                        📷
                    </span>

                    <span>
                        Screenshot ရွေးရန်
                    </span>

                    <small>
                        JPG / PNG
                    </small>
                `;



            window.currentOrder = null;

        }
        catch (error) {

            console.error(
                "Submit Order Error:",
                error
            );

            showAppAlert({
                title: "Error",
                message:
                    error.message ||
                    "Order ပို့ရာတွင် အမှားရှိပါသည်။",
                type: "error"
            });
        }
        finally {

            button.disabled = false;

            button.innerHTML =
                oldText;
        }
    });


document
    .getElementById("buyButton")
    .addEventListener("click", function () {

        const userId =
            document
                .getElementById("userId")
                .value
                .trim();

        if (!/^\d+$/.test(userId)) {

            showAppAlert({
                title: "Warning",
                message: "User ID သည် နံပါတ်များသာ ဖြစ်ရပါမည်။",
                type: "warning"
            });

            return;
        }

        const serverId =
            document
                .getElementById("serverId")
                .value
                .trim();



        if (!/^\d+$/.test(serverId)) {

            showAppAlert({
                title: "Warning",
                message: "Server ID သည် နံပါတ်များသာ ဖြစ်ရပါမည်။",
                type: "warning"
            });

            return;
        }
        // Player ID
        if (!userId) {

            showAppAlert({
                title: "Warning",
                message: "Player ID ထည့်ပေးပါ။",
                type: "warning"
            });

            document
                .getElementById("userId")
                .focus();

            return;
        }


        // Server ID
        if (!serverId) {

            showAppAlert({
                title: "Warning",
                message: "Server ID ထည့်ပေးပါ။",
                type: "warning"
            });

            document
                .getElementById("serverId")
                .focus();

            return;
        }


        // Package
        if (!selectedDiamond) {

            showAppAlert({
                title: "Warning",
                message: "ဝယ်ယူလိုသော Package ကို ရွေးပေးပါ။",
                type: "warning"
            });

            return;
        }



        const packageName =
            selectedDiamond.name
            || "Diamond Package";

        const diamond =
            selectedDiamond.diamond
            || selectedDiamond.total
            || "";

        const price =
            Number(
                selectedDiamond.sellingPrice
                || selectedDiamond.price
                || 0
            );



        document
            .getElementById("paymentUserId")
            .textContent = userId;

        document
            .getElementById("paymentServerId")
            .textContent = serverId;

        document
            .getElementById("paymentPackage")
            .textContent = packageName;

        document
            .getElementById("paymentPrice")
            .textContent =
                price.toLocaleString() + " Ks";

        document
            .getElementById("qrPrice")
            .textContent =
                price.toLocaleString() + " Ks";


        // Save current order
        window.currentOrder = {

            userId: userId,

            serverId: serverId,

            packageName: packageName,

            diamond: diamond,

            sellingPrice: price

        };


        selectedPaymentMethod = null;

        // Hide QR from previous order
        document
            .getElementById("qrSection")
            .style.display = "none";

        // Clear previous QR
        document
            .getElementById("paymentQr")
            .src = "";

        // ==========================================
        // FIRST: Open Payment Method Choose Modal
        // ==========================================

        document
            .getElementById("paymentMethodModal")
            .classList.add("show");
    });


function closePaymentModal() {

    document
        .getElementById("paymentModal")
        .classList.remove("show");
}

document
    .getElementById("paymentScreenshot")
    .addEventListener("change", function () {

        const file = this.files[0];

        if (!file) {
            return;
        }


        // File type
        if (!file.type.startsWith("image/")) {

            showAppAlert({
                title: "Warning",
                message: "Image file တစ်ခုရွေးပေးပါ။",
                type: "warning"
            });

            this.value = "";

            return;
        }


        // 5 MB limit
        if (file.size > 5 * 1024 * 1024) {

            showAppAlert({
                title: "Warning",
                message: "Screenshot size သည် 5MB ထက်မကျော်ရပါ။",
                type: "warning"
            });

            this.value = "";

            return;
        }


        const reader =
            new FileReader();

        reader.onload = function (e) {

            document
                .getElementById("uploadPreview")
                .innerHTML = `

    <img
src = "${e.target.result}"
class="payment-preview" >

    <span>
        ✓ Screenshot ရွေးပြီးပါပြီ
    </span>
`;
        };

        reader.readAsDataURL(file);
    });


let selectedPaymentMethod = null;

// ==========================================
// Choose Payment Method
// ==========================================

function choosePaymentMethod(method) {


selectedPaymentMethod = method;

console.log(
    "Payment Method:",
    selectedPaymentMethod
);


// Close first modal
closePaymentMethodModal();


// Show selected QR
showPaymentQR(method);


}


function showPaymentQR(method) {

const qrImage =
    document.getElementById("paymentQr");

const qrTitle =
    document.getElementById("selectedPaymentTitle");


if (method === "KPay") {

    qrImage.src = "/Image/kpay.jpg";

    qrTitle.innerHTML =
        "💚 KPay ဖြင့် ငွေလွှဲပါ";

}


else if (method === "WavePay") {

    qrImage.src = "/Image/wavepay.jpg";

    qrTitle.innerHTML =
        "💙 WavePay ဖြင့် ငွေလွှဲပါ";

}


// Show QR section
document
    .getElementById("qrSection")
    .style.display = "block";


// Open Payment Modal
document
    .getElementById("paymentModal")
    .classList.add("show");


}

// ==========================================
// Close Payment Method Modal
// ==========================================

function closePaymentMethodModal() {

document
    .getElementById("paymentMethodModal")
    .classList.remove("show");


}
