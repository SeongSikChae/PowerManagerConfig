# PowerManagerConfig

## history

### v1.0.1

* version parameter 요청 시 Revision 정보가 표시되게 기능을 추가하였습니다.

## 실행 파라메터

* --host 접속 하고자 하는 기기 IP (기기 WIFI 접속 시 gateway)
* --port 접속 하고자 하는 기기 PORT (현재 5000번 포트만 이용중인 것으로 파악)
* --api_server_addr 사설 API 인증 서버 URL (ex) https://10.0.0.5:18443
* --web_server_addr 사설 관리 웹서버 URL (ex) https://10.0.0.5/
* --clientCertificate 사설 관리 웹서버에 접속하기 위한 클라이언트 인증서 파일 경로 (p12, pfx, pem 등)
* --clientCertificatePassword 사선 관리 웹서버에 접속하기 위한 클라이언트 인증서 패스워드

## MODE 설명

* V1 - M130, B540 (v1.0.26) 을 위한 설정 모드 (테스트 완료)
* V2 - B540 (v1.0.28) 을 위한 설정 모드 (테스트 필요, 디바이스 없음)
* V3 - B540 (v1.0.30) 을 위한 설정 모드 (테스트 완료)
* V4 - B550 (v1.0.10) 을 위한 설정 모드 (테스트 완료)