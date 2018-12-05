import { connect } from "react-redux";
import { withRouter } from 'react-router-dom';
import { MenuItem } from "../../Common/States/ExtStates";
import { openPage } from "../Actions/AppActions";
import { CommonState } from "../States/AppStates";
import NavMenu, { NavMenuActions, NavMenuProps } from "./NavMenu";

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