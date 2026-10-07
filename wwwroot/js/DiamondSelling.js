let selectedDiamond = null;

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
                    ${Number(item.price).toLocaleString()} Ks
                </div>
`;


            card.addEventListener("click", function () { selectCard(card, { type: "special", packageType: item.type, name: item.name, price: item.price }); });


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
                    ${Number(item.price).toLocaleString()} Ks
                </div>
`;


            // CLICK
            card.addEventListener(
                "click",
                function () {

                    selectCard(
                        card,
                        {
                            type: "recharge",
                            ...item
                        }
                    );

                }
            );


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
    .getElementById("buyButton")
    .addEventListener(
        "click",
        function () {

            const userId =
                document
                    .getElementById("userId")
                    .value
                    .trim();


            const serverId =
                document
                    .getElementById("serverId")
                    .value
                    .trim();


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


            if (!selectedDiamond) {
                showAppAlert({
                    title: "Warning",
                    message: "ဝယ်ယူလိုသော Package ကို ရွေးပေးပါ။",
                    type: "warning"
                });

                

                return;
            }


            const purchaseData = {

                userId: userId,

                serverId: serverId,

                package: selectedDiamond

            };


            console.log(
                "Purchase:",
                purchaseData
            );
             showAppAlert({
                    title: "Warning",
                 message: "ဝယ်ယူမှုကို ဆက်လက်လုပ်ဆောင်နိုင်ပါပြီ။",
                    type: "warning"
                });

        }
    );



document.addEventListener(
    "DOMContentLoaded",
    function () {

        loadDiamondPrices();

    }
);
