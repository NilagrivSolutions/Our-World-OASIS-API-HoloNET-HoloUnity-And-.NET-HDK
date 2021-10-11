import React from "react";
import { BrowserRouter as Router, Route, Switch } from "react-router-dom";

import SideNav from "./common/SideNav";
import Navbar from "./common/Navbar";
import Sidebar from "./common/sidebar/Sidebar";
import PayWithSeeds from "./pages/seeds/PayWithSeeds";
import SendInvite from "./pages/seeds/SendInvite";
import Karma from "./pages/karma/Karma";
import Home from "./pages/Home";
import Login from "./Login";
import Signup from "./Signup";
import AcceptInvite from "./pages/seeds/AcceptInvite";
import AddData from "./popups/data-screen/AddData";
import LoadData from "./popups/data-screen/LoadData";
import OffChainManagement from "./popups/data-screen/OffChainManagement";
import CrossChainManagement from "./popups/data-screen/CrossChainManagement";
import Solana from "./popups/nft/Solana";

import ViewAvatar from "./popups/avatar/viewAvatar";
import AvatarWallet from "./popups/avatar/avatarWallet";
import Message from "./popups/messages/Message";
// import UploadAvatar from "../components/pages/avatar/uploadAvatar";
// import Provider from "../components/pages/providers/Provider";
// import Keymanagement from "../components/pages/providers/KeyManagement";

import "../assets/scss/general.scss";
import "../assets/scss/style.scss";
import "../assets/scss/Seeds.scss";

import axios from "axios";
import 'react-toastify/dist/ReactToastify.css';

class App extends React.Component {
    state = {
        showSidebar: false,
        showLogin: false,
        showSignup: false,
        user: null,

        sidebarMenuOption: [
            {
                data: {
                    loadData: false,
                    sendData: false,
                    manageData: false,
                    offChainManagement: false,
                    crossChainManagement: false
                }
            },
            {
                nft: {
                    solana: false
                }
            }
        ],
    };

    componentDidMount() {
        localStorage.getItem("user");

        if (localStorage.getItem("user")) {
            this.setState({ user: JSON.parse(localStorage.getItem("user")) });
        }
    }

    setUserData = (data) => {
        console.log(data);
        this.setState({
            user: data,
        });
    };

    toggleSidebar = () => {
        this.setState({
            showSidebar: !this.state.showSidebar,
        });
    };

    hidePopups = () => {
        this.setState({
            showLogin: false,
            showSignup: false,
        });
    };

    hideLogin = () => {
        this.setState({
            showLogin: false,
        });
    };

    hideSignup = () => {
        this.setState({
            showSignup: false,
        });
    };

    showLogin = () => {
        this.setState({
            showLogin: true,
            showSignup: false,
        });
    };

    showSignup = () => {
        this.setState({
            showSignup: true,
            showLogin: false,
        });
    };

    handleLogout = () => {
        axios
            .post("https://api.oasisplatform.world/api/avatar/revoke-token", {
                token: this.state.user.jwtToken,
            })
            .then((res) => {
                this.setState({ user: null });
                localStorage.removeItem("user");
                localStorage.removeItem("credentials");
            })
            .catch((err) => {
                this.setState({ user: null });
                localStorage.removeItem("user");
                localStorage.removeItem("credentials");
            });
    };

    toggleScreenPopup = (menuOption, menuName) => {
        let sidebarMenuOption = [...this.state.sidebarMenuOption];

        sidebarMenuOption.map((item) => {
            if(item[menuOption]) {
                item[menuOption][menuName] = !item[menuOption][menuName];
            }
        })

        this.setState({
            sidebarMenuOption
        })
    };

    render() {
        return (
            <div className="main-container">
                <Router>
                    <header>
                        <Navbar
                            showSidebar={this.state.showSidebar}
                            toggleSidebar={this.toggleSidebar}
                            showLogin={this.showLogin}
                            showSignup={this.showSignup}
                            handleLogout={this.handleLogout}
                            user={this.state.user}
                        />
                        {/* <SideNav
                            showSidebar={this.state.showSidebar}
                            toggleSidebar={this.toggleSidebar}
                        /> */}
                        <Sidebar
                            showSidebar={this.state.showSidebar}
                            toggleSidebar={this.toggleSidebar}
                            toggleScreenPopup={this.toggleScreenPopup}
                        />
                    </header>

                    <div className="content-container">
                        <Switch>
                            <Route exact path="/home" component={Home} />

                            <Route path="/pay-with-seeds" component={PayWithSeeds} />
                            <Route path="/donateWithSeeds">
                                <PayWithSeeds seedType="Donate" />
                            </Route>
                            <Route path="/rewardWithSeeds">
                                <PayWithSeeds seedType="Reward" />
                            </Route>
                            <Route
                                path="/accept-invite-to-join-seeds"
                                component={AcceptInvite}
                            />

                            <Route path="/send-invite" component={SendInvite} />
                            <Route exact path="/karma" component={Karma} />
                            <Route exact path="/avatar/view" component={ViewAvatar} />
                            <Route exact path="/avatar/wallet" component={AvatarWallet} />
                            <Route exact path="/message" component={Message}/>

                            {/* <Route exact path="/avatar/upload" component={UploadAvatar} />
                            <Route path="/provider/provider" component={Provider} />
                            <Route
                                path="/provider/key-management"
                                component={Keymanagement}
                            /> */}
                        </Switch>
                    </div>
                </Router>

                <Login
                    className="custom-form"
                    show={this.state.showLogin}
                    hide={this.hideLogin}
                    change={this.showSignup}
                    setUserStateData={this.setUserData}
                />

                <Signup
                    className="custom-form"
                    show={this.state.showSignup}
                    hide={this.hideSignup}
                    change={this.showLogin}
                />

                {/* ========== DATA SCREEN POPUPS START ========== */}
                <AddData
                    show={this.state.sidebarMenuOption[0].data.sendData}
                    hide={this.toggleScreenPopup}
                />

                <LoadData
                    show={this.state.sidebarMenuOption[0].data.loadData}
                    hide={this.toggleScreenPopup}
                />

                <OffChainManagement
                    show={this.state.sidebarMenuOption[0].data.offChainManagement}
                    hide={this.toggleScreenPopup}
                />

                <CrossChainManagement
                    show={this.state.sidebarMenuOption[0].data.crossChainManagement}
                    hide={this.toggleScreenPopup}
                />
                {/* ========== DATA SCREEN POPUPS END ========== */}

                {/* ========== NFT POPUPS START  =========== */}
                <Solana
                    show={this.state.sidebarMenuOption[1].nft.solana}
                    hide={this.toggleScreenPopup}
                />
                {/* ========== NFT POPUPS END  =========== */}

            </div>
        );
    }
}

export default App;
