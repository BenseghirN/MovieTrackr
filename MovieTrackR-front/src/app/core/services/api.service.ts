import { Injectable, inject } from '@angular/core';
import {
	HttpClient,
	HttpHeaders,
	HttpParams,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';

type Primitive = string | number | boolean;
type ParamValue = Primitive | Primitive[];

export interface ApiRequestOptions {
	params?: Record<string, Primitive> | HttpParams;
	headers?: Record<string, string> | HttpHeaders;
	withCredentials?: boolean;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
	private http = inject(HttpClient);

	get<T>(url: string, options?: ApiRequestOptions): Observable<T> {
		return this.http
			.get<T>(url, this.buildOptions(options));
	}

	post<T, B = unknown>(url: string, body: B, options?: ApiRequestOptions): Observable<T> {
		return this.http
			.post<T>(url, body, this.buildOptions(options));
	}

	put<T, B = unknown>(url: string, body: B, options?: ApiRequestOptions): Observable<T> {
		return this.http
			.put<T>(url, body, this.buildOptions(options));
	}

	patch<T, B = unknown>(url: string, body: B, options?: ApiRequestOptions): Observable<T> {
		return this.http
			.patch<T>(url, body, this.buildOptions(options));
	}

	delete<T>(url: string, options?: ApiRequestOptions): Observable<T> {
		return this.http
			.delete<T>(url, this.buildOptions(options));
	}

	private buildOptions(options?: ApiRequestOptions) {
		const httpOptions: {
			params?: HttpParams;
			headers?: HttpHeaders;
			withCredentials?: boolean;
		} = {
            withCredentials: options?.withCredentials ?? true,
        };

		if (options?.params) {
			httpOptions.params = options.params instanceof HttpParams 
				? options.params 
				: this.toHttpParams(options.params);
		}

		httpOptions.headers = options?.headers instanceof HttpHeaders 
			? options.headers 
			: new HttpHeaders(options?.headers ?? {});

		return httpOptions;
	}

	private toHttpParams(params: Record<string, ParamValue> | undefined): HttpParams {
		let httpParams = new HttpParams();
		if (!params) return httpParams;

		for (const [k, v] of Object.entries(params)) {
			if (v === undefined || v === null) continue;

			if (Array.isArray(v)) {
				v.forEach(item => {
					httpParams = httpParams.append(k, String(item));
				});
			} else {
				httpParams = httpParams.set(k, String(v));
			}
		}

		return httpParams;
	}
}
