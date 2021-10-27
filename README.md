# rfe.auth.netcore
1. Downlaod Docker Desktop https://docs.docker.com/desktop/windows/install/
2. Install and get it runnning  
3. ![image](https://user-images.githubusercontent.com/26566374/139065302-d4178e87-ad3c-411c-a800-5b32b5a004bc.png)
4. run docker-compose -f "docker-compose.yml" up -d --build  from the solution directory 
4. alternate method install vscode extension 
  -- ![image](https://user-images.githubusercontent.com/26566374/139065752-10c10e2e-1b9a-470d-9f7e-2a081875f3b1.png)
  -- open docker-compose.yml in vscode, right click and say compose up 
  ![image](https://user-images.githubusercontent.com/26566374/139066132-b779035d-720f-416e-b9cf-6dea7a2082d3.png)
run the following command to transfer db data into the sqlsever database
docker run --rm --volumes-from ms-sql-server -v ${PWD}/data:/backup busybox tar xvf /backup/sqldata.tar 
