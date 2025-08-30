# Pet Management (C# 콘솔) — 초보 개발자의 작은 프로젝트

> 텍스트 파일(petEntry.txt)과 엑셀(petManaged.xlsx)을 읽어 고양이/강아지 입고 → 상태 변경 → 총 관리비 계산을 해주는 C# 콘솔 프로그램입니다.
> EPPlus로 엑셀을 읽고, 간단한 OOP(상속/다형성)를 연습했습니다.

## 주요 기능
- 입고 데이터 로드: petEntry.txt에서 고양이(C)·강아지(D) 정보를 읽어 등록
- 상태 변경 로드(엑셀): 엑셀 1번 시트(인덱스 0)에서 관리번호/상태코드 읽어 반영
- 가격표 로드(엑셀): 엑셀 2번 시트(인덱스 1)에서 서비스 단가 & 할인율 읽기
- 상태/정보 출력: 고양이/강아지 각각 현재 상태를 콘솔에 출력
- 총 관리비 계산: 완료(E)된 아이들에 대해 목욕/커트/발관리 비용 합산 + 3종 세트 할인

## 코드 구조 (OOP)
- abstract class Pet : 공통 속성/메서드 (관리번호, 이름, 나이, 품종, 상태, 서비스유형)
- class PetCat : Pet : 고양이
- class PetDog : Pet : 강아지(강아지는 Psize 추가)
- class ExcelFileHandler : EPPlus로 엑셀 읽기
- class PetManagementSystem : 파일 읽기/상태 반영/비용 계산/전체 실행 흐름
- struct CostType : 코드와 금액(혹은 할인율) 보관
- Program.Main : 시작점

상속/다형성(추상 클래스 + 오버라이드)을 꼭 써보자는 목표로 설계했습니다.

## 구현 포인트
- 상속/다형성 연습: Pet을 추상 클래스로 두고 고양이/강아지에서 PrintInfo() 오버라이드
- EPPlus 활용: 라이브러리로 엑셀 시트 인덱스별 읽기 (ReadDataFromExcel)
- 간단한 검색/갱신: 관리번호로 펫 찾고(Array.Find) 상태 업데이트
- 에러 처리: try/catch로 파일/엑셀 읽기 오류 메시지 표시
