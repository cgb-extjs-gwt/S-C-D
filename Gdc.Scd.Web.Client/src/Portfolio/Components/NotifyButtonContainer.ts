import { connect } from "react-redux";
import { NotifyButtonProps, NotifyButtonView } from "./NotifyButtonView";
import { CommonState } from "../../Layout/States/AppStates";
import * as Permissions from "../../Common/Constants/Permissions";
import { PortfolioService } from "../Services/PortfolioService";
import { NotifyGridActions } from "./NotifyGrid";

const portfolioService = new PortfolioService();

export const NotifyButtonContainer = connect<NotifyButtonProps, NotifyGridActions, {}, CommonState>(
    ({ app }) => ({
        isVisible: !!app.userRoles.find(userRole => userRole.permissions.includes(Permissions.ADMIN)),
        dataLoadUrl: portfolioService.buildGetNewWgsUrl()
    }),
    dispatch => ({
        onWindowNotifyButtonClick: (selectedItems, store) => {
            if (selectedItems.length == 0) {
                Ext.Msg.alert('Message', 'Please select warranty groups');
            } else {
                portfolioService.notifyCountryUsers(selectedItems).then(
                    () => {
                        Ext.Msg.alert('Message', 'Notification successful');
                        store.reload();
                    },
                    () => {
                        Ext.Msg.alert('Error', 'An error occurred during the notification');
                    }
                );
            }
        }
    })
)(NotifyButtonView)