import { buildMvcUrl, get } from "./Ajax";
import { DomainService } from "./DomainService";

export class CacheDomainService<T> {

    private static all: any = {}; // promises cache, singleton

    protected controllerName: string;

    public constructor(cname: string) {
        this.controllerName = cname;
    }

    public getAll(): Promise<T[]> {
        let p = CacheDomainService.all[this.controllerName];
        if (!p) {
            p = new DomainService<T>(this.controllerName).getAll();
            CacheDomainService.all[this.controllerName] = p;
        }
        return p;
    }

    public getFromUrlAll<K>(action: string): Promise<K[]> {
        return this.getFromUrl<K[]>(action);
    }

    public getFromUrl<K>(action: string): Promise<K> {
        let url = buildMvcUrl(this.controllerName, action);
        let p = CacheDomainService.all[url];
        if (!p) {
            p = get<K>(this.controllerName, action);
            CacheDomainService.all[url] = p;
        }
        return p;
    }
}