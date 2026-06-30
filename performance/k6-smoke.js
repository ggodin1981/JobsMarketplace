import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
  vus: 20,
  duration: "30s",
  insecureSkipTLSVerify: true,
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<500"]
  }
};

const baseUrl = __ENV.BASE_URL || "https://host.docker.internal:36060";

export default function () {
  const customerResponse = http.get(`${baseUrl}/customers/search?query=Sm&page=1&pageSize=20`);
  check(customerResponse, {
    "customer search returned 200": (response) => response.status === 200
  });

  const contractorResponse = http.get(`${baseUrl}/contractors/search?query=Build&page=1&pageSize=20`);
  check(contractorResponse, {
    "contractor search returned 200": (response) => response.status === 200
  });

  const jobsResponse = http.get(`${baseUrl}/jobs?page=1&pageSize=20`);
  check(jobsResponse, {
    "jobs endpoint returned 200": (response) => response.status === 200
  });

  sleep(1);
}
