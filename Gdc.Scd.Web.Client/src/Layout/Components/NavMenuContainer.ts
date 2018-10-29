import { connect } from "react-redux";
import NavMenu, { NavMenuProps, NavMenuActions } from "./NavMenu"
import { CommonState } from "../States/AppStates";
import { MenuItem } from "../../Common/States/ExtStates";
import { buildComponentUrl } from "../../Common/Services/Ajax";
import { openPage } from "../Actions/AppActions";
import { withRouter } from 'react-router-dom';

export interface NavMenuContainerProps {
    history?, 
    location?,
    items: MenuItem[]
}

const componentFactory = connect<NavMenuProps, NavMenuActions, NavMenuContainerProps, CommonState>(
    (state, { location, items }) => (<NavMenuProps>{
        items,
        selection: location.pathname,
    }),
    (dispatch, { history }) => ({
        onItemClick: item => {
            history.push(item.id);
            
            dispatch(openPage(item.id));
        }
    }),
    null,
    {
        areStatesEqual: () => true
    }
)


export const NavMenuContainer = withRouter(componentFactory(NavMenu))