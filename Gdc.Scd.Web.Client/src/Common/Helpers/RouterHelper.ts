import { RouteComponentProps } from "react-router";

export const hasQueryParams = (router: RouteComponentProps) => router.history.location.search;

export const deleteQueryParams = (router: RouteComponentProps) => router.history.push(router.history.location.pathname);