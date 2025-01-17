
const state = () => ({
    lastChargedCustomers: [],
  });
  
  const getters = {};
  const actions = {
  };
  
  const mutations = {
    addLastChargedCustomer(state, { customerID, terminalID }) {
        let newArr = [...state.lastChargedCustomers];
        let existingIdx = newArr.findIndex(i => i.id === customerID);
        if (existingIdx > -1){
          newArr.splice(existingIdx, 1);
        }
        newArr = [{
            id: customerID,
            terminalID: terminalID
        }, ...newArr];
        
        if(newArr.length > 5){
            newArr.pop();
        }
        state.lastChargedCustomers = newArr;
    },
  }
  
  export default {
    namespaced: true,
    state,
    getters,
    actions,
    mutations
  }