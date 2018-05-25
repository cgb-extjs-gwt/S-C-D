import * as React from 'react'
import { BrowserRouter as Router } from 'react-router-dom'
import Layout from './Layout/Components/Layout';
import { Provider, connect } from 'react-redux';
import { storeFactory } from './Common/StoreFactory';

declare var Ext:any;

// Enable responsiveConfig app-wide. You can remove this if you don't plan to build a responsive UI.
Ext.require('Ext.plugin.Responsive');

const store = storeFactory();

// const LayoutConnected = connect<{}, LayoutPropsMethods>(
//     null,
//     dispatch => ({
//         onMenuItemClick: (pathName, history) => dispatch(navigateActionCreator(pathName, history))
//     } as LayoutPropsMethods)
// )(Layout)

/**
 * The main application view
 */
const App = () => {
    return (
        //<Provider store={store}>
            <Router>
                <Layout/>
            </Router>
        //</Provider>
    )
}

export default App;
