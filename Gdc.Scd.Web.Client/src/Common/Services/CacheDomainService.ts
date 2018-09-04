import { DomainService } from "./DomainService";

export class CacheDomainService<T> {

    private static all: any = {}; // cache, singleton

    private controllerName: string;

    public constructor(cname: string) {
        this.controllerName = cname;
    }

    public getAll(): Promise<T[]> {
        let data = this.getAllFromCache();
        if (data) {
            return Promise.resolve(data);
        }
        else {
            return new DomainService<T>(this.controllerName).getAll().then(this.onComplete.bind(this));
        }
    }

    private onComplete(d: T[]): T[] {
        this.addAllToCache(d);
        return d;
    }

    private getAllFromCache(): T[] {
        return CacheDomainService.all[this.controllerName];
    }

    private addAllToCache(d: T[]): void {
        CacheDomainService.all[this.controllerName] = d;
    }
}